using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.Data;
using TradingHatsuwa.HubProxy;
using System.Collections.Generic;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;

namespace TradingHatsuwa.Core.ViewModels
{
	public class EventViewModel : ListViewModel<Meeting>
	{
		private readonly IMeetingHubProxy _meetingHub;
		private readonly IEventHubProxy _eventHub;

		private Meeting _meeting;
		private Event _event;
		private System.Threading.Timer _timer;

		/// <summary>
		/// 発話者部分の現在の表示状態を表します。
		/// </summary>
		private enum SpeakerStatus
		{
			/// <summary>何もしていない（誰も発話していない）</summary>
			Idle,

			/// <summary>ルーレット表示中</summary>
			Roulette,

			/// <summary>発話中</summary>
			Speak,
		}

		// 発話者部分の現在の表示状態
		private SpeakerStatus _speakerStatus = SpeakerStatus.Idle;

		/// <summary>
		/// コンストラクタです。
		/// </summary>
		/// <param name="meetingHub"></param>
		/// <param name="eventHub"></param>
		public EventViewModel(IMeetingHubProxy meetingHub, IEventHubProxy eventHub)
		{
			_meetingHub = meetingHub;
			_meetingHub.OnDelete(MeetingHub_OnDelete);

			_eventHub = eventHub;
			_eventHub.OnBeg(EventHub_OnBeg);
			_eventHub.OnUpdate(EventHub_OnUpdate);
		}

		/// <summary>
		/// ビューモデルの初期化時にMvvmCrossから呼ばれます。
		/// </summary>
		/// <param name="parameter"></param>
		public override void Prepare(Meeting parameter)
		{
			_meeting = parameter;
			Title = parameter.Name;
		}

		/// <summary>
		/// ビューモデルの初期化時にMvvmCrossから呼ばれます。
		/// （先にPrepareが呼ばれ、次にInitializeが呼ばれます）
		/// </summary>
		/// <returns></returns>
		public override async Task Initialize()
		{
			_speakerStatus = SpeakerStatus.Idle;
			RenderSpeacker(null);

			// タイマー起動
			_timer = new Timer(OnTimer, null, 0, 100);

			// イベント情報取得
			try
			{
				var source = new CancellationTokenSource();
				var e = await UserDialogs.Instance.LoadingDelayedAsync(_eventHub.GetEvent(_meeting.Id), source);
				if (e == null) return;
				EventHub_OnUpdate(e);
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}

		/// <summary>
		/// 画面が閉じるときにMvvmCrossから呼ばれます。
		/// </summary>
		public override void ViewDisappeared()
		{
			// タイマー破棄
			_timer?.Dispose();
			_timer = null;

			base.ViewDisappeared();
		}

		#region "Properties"

		/// <summary>
		/// 会議を開始します（作成者のみ）
		/// </summary>
		public IMvxAsyncCommand StartCommand => new MvxAsyncCommand(async () =>
		{
			try
			{
				var source = new CancellationTokenSource();
				var success = await UserDialogs.Instance.LoadingDelayedAsync(_eventHub.Start(_meeting.Id, Settings.LoginUser.Id), source);
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}, () => CanStart);

		/// <summary>
		/// 開始ボタンを使用できるかどうかを取得します。
		/// </summary>
		public bool CanStart
		{
			get => _event?.Status == MeetingStatus.Waiting && _meeting.CreatedBy == Settings.LoginUser.Id && ListItems.Count >= 2;
		}

		/// <summary>
		/// 会議中かどうかを取得します。
		/// </summary>
		public bool InMeeting
		{
			get => _event?.Status == MeetingStatus.InMeeting;
		}

		/// <summary>
		/// 会議に参加しているかどうか取得します。
		/// </summary>
		public bool Attended
		{
			get => _event?.Users?.Any(u => u.Id == Settings.LoginUser.Id) ?? false;	// 参加者一覧に自分があれば参加者
		}

		/// <summary>
		/// 発話権の残り枚数を取得します。
		/// </summary>
		public int Tickets
		{
			// 注意: EventUserのTicketsはEventを受け取った時点での残り数
			// その後時間が経ってイベントキューが減っていってもEvent.Ticketsは自動的には減らない
			// なのでイベントキューに残っている自分の発話権 or ルーレット結果を数えてそれを発話券の残り数とする
			get => _event?.QueueItems?.Count(i => i.UserId == Settings.LoginUser.Id && i.RandomlySelected) ?? 0;
		}

		/// <summary>
		/// 発話権が1つ以上あるかどうかを取得します。
		/// </summary>
		public bool HasTicket
		{
			// 注意: EventUserのTicketsはEventを受け取った時点での残り数
			// その後時間が経ってイベントキューが減っていってもEvent.Ticketsは自動的には減らない
			// なのでイベントキューに残っている自分の発話権 or ルーレット結果を数えてそれを発話券の残り数とする
			get => _event?.QueueItems?.Any(i => i.UserId == Settings.LoginUser.Id && i.RandomlySelected) ?? false;
		}

		/// <summary>
		/// 発話権を使用します。
		/// </summary>
		public IMvxAsyncCommand TicketCommand => new MvxAsyncCommand(async () =>
		{
			try
			{
				var source = new CancellationTokenSource();
				var success = await UserDialogs.Instance.LoadingDelayedAsync(_eventHub.UseTicket(_meeting.Id, Settings.LoginUser.Id), source);
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}, () => CanTicketCommand);

		/// <summary>
		/// 発話権ボタンを使用できるかどうかを取得します。
		/// </summary>
		public bool CanTicketCommand => HasTicket && InMeeting && Attended;

		/// <summary>
		/// 時乞いの状態が変わったことを通知します。
		/// （ビュー側でこのイベントを使ってボタンの表示を更新）
		/// </summary>
		public event EventHandler BeggedChanged;

		/// <summary>
		/// 時乞いをオンオフします。
		/// </summary>
		public IMvxAsyncCommand BegCommand => new MvxAsyncCommand(async () =>
		{
			try
			{
				var source = new CancellationTokenSource();
				var success = await UserDialogs.Instance.LoadingDelayedAsync(_eventHub.Beg(_meeting.Id, Settings.LoginUser.Id, !Begged), source);
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}, () => CanBegCommand);

		/// <summary>
		/// 時乞いボタンを使用できるかどうかを取得します。
		/// </summary>
		public bool CanBegCommand => InMeeting && Attended;

		/// <summary>
		/// 時乞いの状態を取得します。
		/// </summary>
		public bool Begged
		{
			get => _event?.Users?.SingleOrDefault(u => u.Id == Settings.LoginUser.Id)?.Begged ?? false;
		}

		/// <summary>
		/// 発話振興券の残り数を取得します。
		/// </summary>
		public int Coupons
		{
			get => _event?.Users?.SingleOrDefault(u => u.Id == Settings.LoginUser.Id)?.Coupons ?? 0;
		}

		/// <summary>
		/// 発話振興券が1つ以上あるかどうかを取得します。
		/// </summary>
		public bool HasCoupon
		{
			get => _event?.Users?.SingleOrDefault(u => u.Id == Settings.LoginUser.Id)?.Coupons > 0;
		}

		/// <summary>
		/// 発話振興券のクリックイベントです。
		/// </summary>
		public IMvxCommand CouponCommand => new MvxCommand(() =>
		{
			// 何もしない
			// 発話振興券は参加者アイコンをクリックしてその人に与えるのでUserItemClick()の方で処理している
		}, () => CanCouponCommand);

		/// <summary>
		/// 発話振興券ボタンを使用できるかどうかを取得します。
		/// </summary>
		public bool CanCouponCommand => HasCoupon && InMeeting && Attended;

		/// <summary>
		/// Doneボタンのクリックイベントです。
		/// </summary>
		public IMvxAsyncCommand DoneCommand => new MvxAsyncCommand(async () =>
		{
			try
			{
				var source = new CancellationTokenSource();
				var success = await UserDialogs.Instance.LoadingDelayedAsync(_eventHub.Done(_meeting.Id, Settings.LoginUser.Id), source);
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}, () => Done);

		/// <summary>
		/// Doneボタンを使用できるかどうかを取得します。
		/// </summary>
		public bool Done
		{
			get
			{
				// Doneボタンが使えるのは自分が発話しているとき
				if (!InMeeting || _event == null)
				{
					return false;
				}

				var top = _event.QueueItems.FirstOrDefault();
				if (top == null || top.UserId != Settings.LoginUser.Id)
				{
					return false;
				}

				var now = DateTime.UtcNow;
				if (now <= top.StartTime && top.EndTime < now)
				{
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// 発話中のユーザーアイコンを取得、もしくは、設定します。
		/// </summary>
		public string SpeakerImage
		{
			get => _speakerImage;
			set => SetProperty(ref _speakerImage, value);
		}
		private string _speakerImage;

		/// <summary>
		/// 発話の残り時間を取得、もしくは、設定します。
		/// </summary>
		public string TimeLeft
		{
			get => _timeLeft;
			set => SetProperty(ref _timeLeft, value);
		}
		private string _timeLeft;

		/// <summary>
		/// 経過(ProgressBar)を取得、もしくは、設定します。
		/// </summary>
		public int Progress
		{
			get => _progress;
			set => SetProperty(ref _progress, value);
		}
		private int _progress;

		/// <summary>
		/// 発話待ちユーザーのコレクションを取得します。
		/// </summary>
		public MvxObservableCollection<ListItemViewModel> Queue { get; } = new MvxObservableCollection<ListItemViewModel>();

		/// <summary>
		/// ユーザーアイコンのクリックイベントです。
		/// </summary>
		/// <param name="userId"></param>
		private async void UserItemClick(int userId)
		{
			if (!InMeeting)
			{
				// ミーティングが始まっていない
				return;
			}
			if (userId == Settings.LoginUser.Id)
			{
				// 自分をクリックしたときは何もしない
				return;
			}

			var item = ListItems.Cast<EventUserListItemViewModel>().First(u => u.UserId == userId);
			if (item.Coupons > 0)
			{
				// 発話振興券を使う
				var config = new ConfirmConfig
				{
					Message = "発話振興券を使って発話権を譲渡しますか？",
					OkText = "発話振興券を使う",
				};
				var ok = await UserDialogs.Instance.ConfirmAsync(config);
				if (!ok)
				{
					return;
				}
			}
			else if (item.Tickets > 0)
			{
				// 自分の発話権を譲渡する
				var config = new ConfirmConfig
				{
					Message = "発話権を譲渡しますか？",
					OkText = "発話権を譲渡",
				};
				var ok = await UserDialogs.Instance.ConfirmAsync(config);
				if (!ok)
				{
					return;
				}
			}

			try
			{
				var source = new CancellationTokenSource();
				var results = await UserDialogs.Instance.LoadingDelayedAsync(_eventHub.UseCoupon(_meeting.Id, Settings.LoginUser.Id, userId), source);
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}

		#endregion

		#region "MeetingHub"

		private async void MeetingHub_OnDelete(Meeting m)
		{
			if (m.Id != _meeting.Id) return;

			UserDialogs.Instance.Alert("この会議は削除されました");
			await NavigationService.Close(this);
		}

		#endregion

		#region "EventHub"

		/// <summary>
		/// EventHubのOnBegを処理します。
		/// </summary>
		/// <param name="b"></param>
		private void EventHub_OnBeg(Beggar b)
		{
			var user = _event.Users.SingleOrDefault(x => x.Id == b.UserId);
			if (user != null)
			{
				user.Begged = b.Begged;
			}

			var eu = ListItems.Cast<EventUserListItemViewModel>().SingleOrDefault(x => x.UserId == b.UserId);
			if (eu != null)
			{
				eu.SmallIcon = b.Begged ? "begged" : null;
			}

			if (user.Id == Settings.LoginUser.Id)
			{
				// 自分が変化したとき
				BeggedChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// EventHubのOnUpdateを処理します。
		/// </summary>
		/// <param name="e"></param>
		private void EventHub_OnUpdate(Event e)
		{
			if (e.MeetingId != _meeting.Id) return;

			// ビューを操作するのでメインスレッドに移る必要あり（SignalRのコールバックはバックグラウンドスレッドで呼ばれる）
			Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
			{
				_event = e;

				// 参加ユーザーアイコン
				ListItems.Clear();
				foreach (var user in e.Users)
				{
					var item = new EventUserListItemViewModel
					{
						UserId = user.Id,
						Text = user.Name,
						Image = user.IconUrl(),
						SmallIcon = user.Begged ? "begged" : null,
						Tickets = user.Tickets,
						Coupons = user.Coupons,
						Selectable = true,
						OnAction = () => UserItemClick(user.Id),
					};
					ListItems.Add(item);
				}

				RaisePropertyChanged(() => CanStart);
				RaisePropertyChanged(() => InMeeting);
				RaisePropertyChanged(() => Attended);
				RaisePropertyChanged(() => Begged);
				RaisePropertyChanged(() => CanBegCommand);

				RaisePropertyChanged(() => Tickets);
				RaisePropertyChanged(() => HasTicket);
				RaisePropertyChanged(() => CanTicketCommand);
				RaisePropertyChanged(() => Coupons);
				RaisePropertyChanged(() => HasCoupon);
				RaisePropertyChanged(() => CanCouponCommand);
				RaisePropertyChanged(() => Done);
			});
		}

		/// <summary>
		/// タイマーから100ms間隔で呼ばれます。
		/// </summary>
		/// <param name="status"></param>
		private void OnTimer(object status)
		{
			// ビューを操作するのでメインスレッドに移る必要あり（SignalRのコールバックはバックグラウンドスレッドで呼ばれる）
			Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(async () =>
			{
				if (_event == null)
				{
					return;
				}

				var prevTop = _event.QueueItems.FirstOrDefault();
				var prevStatus = _speakerStatus;

				var now = DateTime.UtcNow;
				_event.QueueItems = _event.QueueItems.SkipWhile(i => i.EndTime < now).ToList();	// 終了したものを取り除く
				var top = _event.QueueItems.FirstOrDefault();
				if (top == null)
				{
					// すべて終了している
					RenderSpeacker(null);
					Queue.Clear();
					_speakerStatus = SpeakerStatus.Idle;
				}
				else
				{
					if (top.StartTime <= now)
					{
						// 発話中
						RenderSpeacker(top);
						RenderQueue(_event.QueueItems.Skip(1));
						_speakerStatus = SpeakerStatus.Speak;
					}
					else
					{
						// まだ次の人の発話時間になっていない
						RenderSpeacker(null);
						if (top.RandomlySelected)
						{
							// 次の人がルーレット
							if (_speakerStatus != SpeakerStatus.Roulette)
							{
								// ルーレット開始
								RenderSpeacker(null);
								Queue.Clear();
								_speakerStatus = SpeakerStatus.Roulette;
								await PlayRoulette(top.UserId, _event.Users);
							}
						}
						else
						{
							_speakerStatus = SpeakerStatus.Idle;
						}
						RenderQueue(_event.QueueItems);
					}
				}

				if (prevTop != _event.QueueItems.FirstOrDefault()
				|| prevStatus != _speakerStatus)
				{
					// 状態が変わったらボタンなどを更新する（毎回すると重そうなので）
					foreach (var vm in ListItems.Cast<EventUserListItemViewModel>())
					{
						// 発話権の残り数を更新
						vm.Tickets = _event.QueueItems.Count(i => i.UserId == vm.UserId && i.RandomlySelected);
					}

					RaisePropertyChanged(() => Tickets);
					RaisePropertyChanged(() => HasTicket);
					RaisePropertyChanged(() => CanTicketCommand);
					RaisePropertyChanged(() => Coupons);
					RaisePropertyChanged(() => HasCoupon);
					RaisePropertyChanged(() => CanCouponCommand);
					RaisePropertyChanged(() => Done);
				}
			});
		}

		#endregion

		/// <summary>
		/// 発話者を表示します。
		/// </summary>
		/// <param name="item">表示する発話者のキューアイテム。</param>
		private void RenderSpeacker(EventQueueItem item)
		{
			if (item == null)
			{
				SpeakerImage = "";
				TimeLeft = "";
				Progress = 0;
			}
			else
			{
				SpeakerImage = _event.Users.SingleOrDefault(u => u.Id == item.UserId)?.IconUrl();
				var tl = item.EndTime - DateTime.UtcNow;
				TimeLeft = tl.ToString(@"mm\:ss");
				Progress = (int)(tl.TotalSeconds / (item.EndTime - item.StartTime).TotalSeconds * 100.0);
			}
		}

		/// <summary>
		/// 発話待ちのキューを表示します。
		/// </summary>
		/// <param name="queue">発話待ちのキュー。</param>
		private void RenderQueue(IEnumerable<EventQueueItem> queue)
		{
			// ここはタイマーから頻繁に呼ばれる
			// 毎回ビューモデルを作成し直すと重そうなので数か内容が変わったときのみ更新するようにする

			// まずキューに入るユーザーアイコンURLのリストを作る（キューは3つまで）
			var urls = new List<string>();
			foreach (var item in queue.Where(x => !x.RandomlySelected))
			{
				urls.Add(_event.Users.SingleOrDefault(u => u.Id == item.UserId).IconUrl());
				if (urls.Count >= 3)
				{
					break;
				}
			}

			// キューの内容が変わったか
			var changed = false;
			if (Queue.Count != urls.Count)
			{
				changed = true;
			}
			else
			{
				for (var i = 0; i < urls.Count; ++i)
				{
					if (urls[i] != Queue[i].Image)
					{
						changed = true;
						break;
					}
				}
			}
			if (changed)
			{ 
				// キューの内容が変わってるので再作成
				Queue.Clear();
				foreach (var u in urls)
				{
					Queue.Add(new ListItemViewModel()
					{
						Image = u,
						Selectable = false,
					});
				}
			}
		}

		/// <summary>
		/// ルーレットを表示します。
		/// </summary>
		/// <param name="selectedUserId">当選するユーザー。</param>
		/// <param name="users">ルーレットに表示するユーザーのコレクション。</param>
		/// <returns></returns>
		private async Task<bool> PlayRoulette(int selectedUserId, IEnumerable<EventUser> users)
		{
			var options = new RouletteOptions
			{
				Users = users.ToList(),
				SelectedUserId = selectedUserId,
			};
			await NavigationService.Navigate(typeof(RouletteViewModel), options);
			return true;
		}
	}

	/// <summary>
	/// ユーザーアイコンのビューモデルです。
	/// </summary>
	public class EventUserListItemViewModel : ListItemViewModel
	{
		public int UserId
		{
			get => _userId;
			set => SetProperty(ref _userId, value);
		}
		private int _userId;

		public string SmallIcon
		{
			get => _image;
			set => SetProperty(ref _image, value);
		}
		private string _image;

		public int Tickets
		{
			get => _tickets;
			set => SetProperty(ref _tickets, value);
		}
		private int _tickets;

		public int Coupons
		{
			get => _coupons;
			set => SetProperty(ref _coupons, value);
		}
		private int _coupons;
	}
}