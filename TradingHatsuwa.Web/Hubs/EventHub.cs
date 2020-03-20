using System.Collections.Generic;
using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TradingHatsuwa.Web.Helpers;
using TradingHatsuwa.Web.Models;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;

namespace TradingHatsuwa.Web
{
    [HubName("EventHub")]
    public class EventHub : Hub
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

		/// <summary>
		/// [Hubのエントリーポイント] 会議に参加します。
		/// </summary>
		/// <param name="meetingId">会議。</param>
		/// <param name="userId">参加するユーザー。</param>
		/// <param name="attended">参加者か立ち見か。</param>
		/// <returns></returns>
        public bool Enter(int meetingId, int userId, bool attended)
        {
            var meeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
            if (meeting == null)
                return false; // invalid ID

            Groups.Add(Context.ConnectionId, $"{meetingId}");

            if (!attended)
                return true; // 立ち見

            var user = db.Users.SingleOrDefault(i => i.Id == userId && !i.Deleted);
            if (user == null)
                return false; // invalid ID

            var eventUser = db.EventUsers.SingleOrDefault(i => i.Meeting.Id == meeting.Id && i.User.Id == user.Id);
            if (eventUser != null)
                return true; // 参加済み

            if (meeting.Status == Data.MeetingStatus.Waiting)
            {
                var newEventUser = new EventUser
                {
                    Meeting = meeting,
                    User = user,
                    Tickets = meeting.Tickets,
                    Coupons = meeting.Coupons,
                };

                meeting.Members.Add(user);
                db.EventUsers.Add(newEventUser);
                db.SaveChanges();

                NotifyEvent(meeting);

                return true;
            }

            return false; // Waiting 以外で参加しようとした
        }

		/// <summary>
		/// [Hubのエントリーポイント] 会議から抜けます。
		/// </summary>
		/// <param name="meetingId">会議。</param>
		/// <param name="userId">抜けるユーザー。</param>
		/// <param name="attended">参加者か立ち見か。</param>
		/// <returns></returns>
		public bool Exit(int meetingId, int userId, bool attended)
        {
            Groups.Remove(Context.ConnectionId, $"{meetingId}");
            return true;
        }

		/// <summary>
		/// [Hubのエントリーポイント] 会議を開始します。
		/// </summary>
		/// <param name="meetingId">会議。</param>
		/// <param name="userId">参加するユーザー。</param>
		/// <returns></returns>
		public bool Start(int meetingId, int userId)
        {
            var meeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
            if (meeting == null)
                return false; // invalid ID

            var user = db.Users.SingleOrDefault(i => i.Id == userId && !i.Deleted);
            if (user == null)
                return false; // invalid ID

            if (meeting.CreatedBy.Id != user.Id)
                return false; // 会議作成者ではない

            if (meeting.Status == Data.MeetingStatus.Waiting)
            {                
                meeting.Status = Data.MeetingStatus.InMeeting;
                db.SaveChanges();

				Task.Run(() => CreateEventQueue(meeting));
                return true;
            }
            else if (meeting.Status == Data.MeetingStatus.InMeeting)
            {
                return true;
            }
            return false;
        }

		/// <summary>
		/// [Hubのエントリーポイント] 発話権を使用します。
		/// </summary>
		/// <param name="meetingId">会議。</param>
		/// <param name="userId">発話権を使用するユーザー。</param>
		/// <returns></returns>
		public bool UseTicket(int meetingId, int userId)
		{
			var meeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
			if (meeting == null)
				return false; // invalid ID

			var user = db.Users.SingleOrDefault(i => i.Id == userId && !i.Deleted);
			if (user == null)
				return false; // invalid ID

			Task.Run(() => UpdateEventQueueWhenUseTicket(meeting, user));
			return true;
		}

		/// <summary>
		/// [Hubのエントリーポイント] 発話権を譲渡します。
		/// </summary>
		/// <param name="meetingId">会議。</param>
		/// <param name="userId">発話権の譲渡元のユーザー。</param>
		/// <param name="targetUserId">譲渡先のユーザー。</param>
		/// <returns></returns>
		public bool UseCoupon(int meetingId, int userId, int targetUserId)
		{
			var meeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
			if (meeting == null)
				return false; // invalid ID

			var user = db.Users.SingleOrDefault(i => i.Id == userId && !i.Deleted);
			if (user == null)
				return false; // invalid ID

			Task.Run(() => UpdateEventQueueWhenUseCoupon(meeting, user, targetUserId));
			return true;
		}

		/// <summary>
		/// [Hubのエントリーポイント] 発話を終了します。
		/// </summary>
		/// <param name="meetingId">会議。</param>
		/// <param name="userId">発話が終了したユーザー。</param>
		/// <returns></returns>
		public bool Done(int meetingId, int userId)
		{
			var meeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
			if (meeting == null)
				return false; // invalid ID

			var user = db.Users.SingleOrDefault(i => i.Id == userId && !i.Deleted);
			if (user == null)
				return false; // invalid ID

			Task.Run(() => UpdateEventQueueWhenDone(meeting, user));
			return true;
		}

		/// <summary>
		/// [Hubのエントリーポイント] 時乞いします。
		/// </summary>
		/// <param name="meetingId">会議。</param>
		/// <param name="userId">時乞いするユーザー。</param>
		/// <param name="begged">trueのとき時乞いしている。falseのときしていない。</param>
		/// <returns></returns>
		public bool Beg(int meetingId, int userId, bool begged)
        {
			// DB更新
			using (var transaction = db.Database.BeginTransaction())
			{
				try
				{
					var eventUser = db.EventUsers.SingleOrDefault(u => u.Meeting.Id == meetingId && u.User.Id == userId);
					if (eventUser == null)
						return false; // invalid ID

					eventUser.Begged = begged;

					db.SaveChanges();
					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
					return false;
				}
			}

			// 配信
			Clients.Group($"{meetingId}").Beg(new Data.Beggar
            {
                UserId = userId,
                Begged = begged,
            });

            return true;
        }

		/// <summary>
		/// 指定された会議に参加している全員に会議情報（ユーザーとイベントキュー）を配信します。
		/// </summary>
		/// <param name="meeting">会議。</param>
        private void NotifyEvent(Meeting meeting)
        {
            var ev = CreateEvent(meeting);
            if (ev != null)
                Clients.Group($"{meeting.Id}").Update(ev);
        }

		// ルーレットの表示時間（ミリ秒）（クライアントアプリの実装に依存する）
		private const int c_rouletteDueTime = 10000;

		/// <summary>
		/// 初期状態のイベントキュー（発話待ち+ルーレット結果）を作成します。
		/// </summary>
		/// <param name="meeting">会議</param>
		private void CreateEventQueue(Meeting meeting)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				try
				{
					// まず全削除する
					db.EventQueueItems.RemoveRange(db.EventQueueItems.Where(i => i.Meeting.Id == meeting.Id));

					// ユーザーのリスト
					var users = db.EventUsers.Where(i => i.Meeting.Id == meeting.Id).Select(i => i.User).ToList();

					var startTime = DateTime.UtcNow
						.AddSeconds(meeting.IdleSeconds)
						.AddMilliseconds(c_rouletteDueTime);
					for (var i = 0; i < meeting.Tickets; ++i)
					{
						foreach (var user in users.OrderBy(_ => Guid.NewGuid()))    // GUIDを使ったシャッフル（簡単かつ高精度らしい）
						{
							var endTime = startTime.AddSeconds(meeting.Seconds);
							db.EventQueueItems.Add(new EventQueueItem()
							{
								Meeting = meeting,
								User = user,
								RandomlySelected = true,
								StartTime = startTime,
								EndTime = endTime,
							});
							startTime = endTime.AddMilliseconds(c_rouletteDueTime);
						}
					}

					db.SaveChanges();
					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
					return;
				}
			}
			NotifyEvent(meeting);
		}

		/// <summary>
		/// 発話権行使時のイベントキュー（発話待ち+ルーレット結果）を作成します。
		/// </summary>
		/// <param name="meeting">会議</param>
		/// <param name="user">今回発話権を行使したユーザー</param>
		private void UpdateEventQueueWhenUseTicket(Meeting meeting, User user)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				try
				{
					// 終わっていないものを取得（発話中のものも含む）（DBからデタッチした状態にしたいのでToXferData()する）
					var now = DateTime.UtcNow;
					var items = db.EventQueueItems.Where(i => i.Meeting.Id == meeting.Id && now < i.EndTime).OrderBy(i => i.Id).ToList().Select(i => i.ToXferData()).ToList();

					// ルーレット結果の末尾から該当ユーザーのものを探して、これを発話待ちの最後に移動する
					var item = items.LastOrDefault(i => i.UserId == user.Id && i.RandomlySelected);
					if (item != null)
					{
						if (object.ReferenceEquals(item, items[0]))
						{
							// 自分しか残っていない
							item.StartTime = now;
							item.EndTime = now.AddMilliseconds(meeting.Seconds);
							item.RandomlySelected = false;
						}
						else
						{
							var top = 0;
							if (now < items[0].EndTime)
							{
								// 最初のアイテムはすでに発話中なのでこれ以降のルーレット結果の最初を探す
								top = items.Skip(1).ToList().FindIndex(i => i.RandomlySelected);    // ルーレット結果の最初（少なくとも自分の分はあるはずなのでみつからないことはありえない）
							}
							else
							{
								top = items.FindIndex(i => i.RandomlySelected);    // ルーレット結果の最初（少なくとも自分の分はあるはずなのでみつからないことはありえない）
							}
							var startTime = now;
							if (top >= 1)
							{
								startTime = items[top - 1].EndTime;
							}
							items.Remove(item);
							items.Insert(top, item);
							item.RandomlySelected = false;

							// 開始終了時刻をつけ直す
							for (var i = top; i < items.Count; ++i)
							{
								if (items[i].RandomlySelected)
								{
									startTime = startTime.AddMilliseconds(c_rouletteDueTime);
								}
								items[i].StartTime = startTime;
								startTime = startTime.AddSeconds(meeting.Seconds);
								items[i].EndTime = startTime;
							}
						}

						// DBに保存
						SaveEventQueue(meeting, items);
					}
					else
					{
						// 発話権が残っていない
						// 例外を出すわけにもいかないのでとりあえず何もしない
					}

					db.SaveChanges();
					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
					return;
				}
			}
			NotifyEvent(meeting);
		}

		/// <summary>
		/// 発話終了ボタンクリック時のイベントキュー（発話待ち+ルーレット結果）を作成します。
		/// </summary>
		/// <param name="meeting">会議</param>
		/// <param name="user">発話が終了したユーザー</param>
		private void UpdateEventQueueWhenDone(Meeting meeting, User user)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				try
				{
					// 終わっていないものを取得（発話中のものも含む）（DBからデタッチした状態にしたいのでToXferData()する）
					var now = DateTime.UtcNow;
					var items = db.EventQueueItems.Where(i => i.Meeting.Id == meeting.Id && now <= i.EndTime).OrderBy(i => i.Id).ToList().Select(i => i.ToXferData()).ToList();

					// 最初のものが該当ユーザーのものだったら削除する
					var item = items.FirstOrDefault();
					if (item != null)
					{
						items.Remove(item);

						// 開始終了時刻をつけ直す
						var startTime = now;
						for (var i = 0; i < items.Count; ++i)
						{
							if (items[i].RandomlySelected)
							{
								startTime = startTime.AddMilliseconds(c_rouletteDueTime);
							}
							items[i].StartTime = startTime;
							startTime = startTime.AddSeconds(meeting.Seconds);
							items[i].EndTime = startTime;
						}

						// DBに保存
						SaveEventQueue(meeting, items);
					}
					else
					{
						// 最初のものが該当ユーザーのものではなかった
						// 例外を出すわけにもいかないのでとりあえず何もしない
					}

					db.SaveChanges();
					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
					return;
				}
			}
			NotifyEvent(meeting);
		}

		/// <summary>
		/// 発話権譲渡時のイベントキュー（発話待ち+ルーレット結果）を作成します。
		/// </summary>
		/// <param name="meeting">会議</param>
		/// <param name="user">発話権の譲渡元のユーザー</param>
		/// <param name="targetUserId">譲渡先のユーザー。</param>
		private void UpdateEventQueueWhenUseCoupon(Meeting meeting, User user, int targetUserId)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				try
				{
					// 終わっていないものを取得（発話中のものも含む）（DBからデタッチした状態にしたいのでToXferData()する）
					var now = DateTime.UtcNow;
					var items = db.EventQueueItems.Where(i => i.Meeting.Id == meeting.Id && now <= i.EndTime).OrderBy(i => i.Id).ToList().Select(i => i.ToXferData()).ToList();

					var top = -1;
					var eventUser = db.EventUsers.SingleOrDefault(u => u.Meeting.Id == meeting.Id && u.User.Id == user.Id);
					if (eventUser.Coupons > 0)
					{
						// 発話振興券を使って譲渡
						--eventUser.Coupons;

						top = items.Count;		// 開始終了時刻をつけ直すのは新しく追加したものだけ
					}
					else
					{
						// 発話権を譲渡

						// ルーレット結果の末尾から該当ユーザーのものを探して、これを削除する
						var item = items.LastOrDefault(i => i.UserId == user.Id && i.RandomlySelected);
						if (item != null)
						{
							top = items.IndexOf(item);  // 削除するところ以降の開始終了時刻をつけ直す
							items.Remove(item);
						}
						else
						{
							// 発話権が残っていない
							// 例外を出すわけにもいかないのでとりあえず何もしない
						}
					}

					if (top >= 0)
					{
						// 新しくイベントキューの末尾に追加
						items.Add(new Data.EventQueueItem()
						{
							UserId = targetUserId,
							RandomlySelected = true,
						});

						var startTime = DateTime.UtcNow;
						if (top >= 1)
						{
							startTime = items[top - 1].EndTime;
						}

						// 開始終了時刻をつけ直す
						for (var i = top; i < items.Count; ++i)
						{
							if (items[i].RandomlySelected)
							{
								startTime = startTime.AddMilliseconds(c_rouletteDueTime);
							}
							items[i].StartTime = startTime;
							startTime = startTime.AddSeconds(meeting.Seconds);
							items[i].EndTime = startTime;
						}

						// DBに保存
						SaveEventQueue(meeting, items);
					}

					db.SaveChanges();
					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
					return;
				}
			}
			NotifyEvent(meeting);
		}

		/// <summary>
		/// イベントキュー（発話待ち+ルーレット結果）を保存します。
		/// </summary>
		/// <param name="meeting">会議</param>
		/// <param name="items">イベントキューに保存するデータ</param>
		private void SaveEventQueue(Meeting meeting, IList<Data.EventQueueItem> items)
		{
			// ユーザーのリスト
			var eventUsers = db.EventUsers.Where(i => i.Meeting.Id == meeting.Id).ToList();

			// 発話権の数を更新（キューに残っているものが発話権の数）
			foreach (var eventUser in eventUsers)
			{
				eventUser.Tickets = items.Count(i => i.UserId == eventUser.User.Id);
			}

			// 全削除する
			db.EventQueueItems.RemoveRange(db.EventQueueItems.Where(i => i.Meeting.Id == meeting.Id));

			// イベントキュー保存
			foreach (var i in items)
			{
				db.EventQueueItems.Add(new EventQueueItem()
				{
					Meeting = meeting,
					User = eventUsers.First(u => u.User.Id == i.UserId).User,
					RandomlySelected = i.RandomlySelected,
					StartTime = i.StartTime,
					EndTime = i.EndTime,
				});
			}
		}

		private Data.Event CreateEvent(Meeting meeting)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var users = db.EventUsers.Where(i => i.Meeting.Id == meeting.Id).ToList().Select(i => i.ToXferData());
                    var items = db.EventQueueItems.Where(i => i.Meeting.Id == meeting.Id).ToList().Select(i => i.ToXferData());

                    var ev = new Data.Event
                    {
                        MeetingId = meeting.Id,
						Status = meeting.Status,
                        Users = users,
                        QueueItems = items,
                    };

                    return ev;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
            return null;
        }


        public Data.Event GetEvent(int meetingId)
        {
            var meeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
            if (meeting == null)
                return null; // invalid ID

            return CreateEvent(meeting);
        }

    }
}