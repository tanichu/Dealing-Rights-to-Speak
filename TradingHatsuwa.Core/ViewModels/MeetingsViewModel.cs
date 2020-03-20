using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.Data;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.Core.ViewModels
{
	public class MeetingsViewModel : ListViewModel
    {
        private readonly IMeetingHubProxy _meetingHub;
		private readonly IEventHubProxy _eventHub;

		public MeetingsViewModel(IMeetingHubProxy meetingHub, IEventHubProxy eventHub)
        {
            _meetingHub = meetingHub;
            _meetingHub.OnCreate(MeetingHub_OnCreate);
            _meetingHub.OnUpdate(MeetingHub_OnUpdate);
            _meetingHub.OnDelete(MeetingHub_OnDelete);

			_eventHub = eventHub;

			Title = "会議一覧";
        }

        public IMvxAsyncCommand AddCommand => new MvxAsyncCommand(async () =>
        {
            var result = await NavigationService.Navigate<MeetingCreationViewModel, Meeting>();
            if (result == null) return; // cancel

            var item = CreateListItem(result);
            ListItems.Insert(0, item);
        });

        public override async Task Initialize()
        {
            await CreateListItemsAsync();
        }

        private void MeetingHub_OnCreate(Meeting m)
        {
            var item = ListItems.SingleOrDefault(i => i.GetValue<Meeting>().Id == m.Id);
            if (item != null) return; // already exists

            ListItems.Insert(0, CreateListItem(m));
        }

        private void MeetingHub_OnUpdate(Meeting m)
        {
            var item = ListItems.SingleOrDefault(i => i.GetValue<Meeting>().Id == m.Id);
            if (item == null) return;
            var index = ListItems.IndexOf(item);

            ListItems.ReplaceRange(new[] { CreateListItem(m) }, index, 1);
        }

        private void MeetingHub_OnDelete(Meeting m)
        {
            var item = ListItems.SingleOrDefault(i => i.GetValue<Meeting>().Id == m.Id);
            if (item == null) return;
            ListItems.Remove(item);
        }

        private async Task CreateListItemsAsync()
        {
            try
            {
                var source = new CancellationTokenSource();
                var meetings = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.GetMeetings(), source);
                if (meetings == null) return;

                foreach (var m in meetings)
                {
                    var item = CreateListItem(m);
                    ListItems.Add(item);
                }
            }
            catch (AggregateException) { return; }
            catch (TaskCanceledException) { return; }
        }

        private async Task DeleteAsync(MeetingListItemViewModel item)
        {
            try
            {
                var meeting = item.GetValue<Meeting>();

                var source = new CancellationTokenSource();
                var success = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.Delete(meeting), source);
                if (success)
                {
                    ListItems.Remove(item);
                    UserDialogs.Instance.Toast("削除しました");
                }
            }
            catch (AggregateException) { return; }
            catch (TaskCanceledException) { return; }
        }

		private async Task<bool> EnterAsync(int meetingId, int userId, bool attended)
		{
			try
			{
				var source = new CancellationTokenSource();
				var success = await UserDialogs.Instance.LoadingDelayedAsync(_eventHub.Enter(meetingId, userId, attended), source);
				if (!success)
				{
					UserDialogs.Instance.Toast("参加できません");
					return false;
				}
				return true;
			}
			catch (AggregateException) { return false; }
			catch (TaskCanceledException) { return false; }
		}

		private async Task<bool> FinishEvaluationAsync(MeetingListItemViewModel item)
		{
			try
			{
				var meeting = item.GetValue<Meeting>();
				meeting.Status = MeetingStatus.Closed;

				var source = new CancellationTokenSource();
				return await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.Update(meeting), source);
			}
			catch (AggregateException) { return false; }
			catch (TaskCanceledException) { return false; }
		}

		private MeetingListItemViewModel CreateListItem(Meeting m)
        {
            var isOwner = m.CreatedBy == Settings.LoginUser?.Id;

            var item = new MeetingListItemViewModel
            {
                Text = m.Name,
                DetailText = $"発話権: {m.Tickets}枚 ({m.Seconds}秒/枚), 振興券: {m.Coupons}枚",
                Selectable = true,
                Image =
                    m.Status == MeetingStatus.Waiting ? "meeting_blue" :
                    m.Status == MeetingStatus.InMeeting ? "meeting_red" :
                    m.Status == MeetingStatus.Evaluating ? "meeting_yellow" :
                    m.Status == MeetingStatus.Closed ? "meeting_grey" : "",
                SmallIcon =
                    m.Status == MeetingStatus.Evaluating ? "ic_lead_pencil_black_24dp" :
                    m.Status == MeetingStatus.Closed ? "ic_trophy_variant_black_24dp" : null,
                Value = m,
            };
            switch (m.Status)
            {
                case MeetingStatus.Waiting:
                    item.OnAction = () =>
                    {
                        var config = new ActionSheetConfig();
						config.Add("参加する", async () =>
						{
							if(await EnterAsync(m.Id, Settings.LoginUser.Id, true))
							{
								await NavigationService.Navigate(typeof(EventViewModel), m);
							}
                        });
                        config.Add("立ち見する", async () =>
                        {
							await EnterAsync(m.Id, Settings.LoginUser.Id, false);
							await NavigationService.Navigate(typeof(EventViewModel), m);
                        });

                        if (isOwner)
                        {
                            config.SetDestructive("削除", async () => await DeleteAsync(item));
                        }

                        config.SetCancel();
                        UserDialogs.Instance.ActionSheet(config);
                    };
                    break;
                case MeetingStatus.InMeeting:
                    item.OnAction = async () =>
                    {
                        await NavigationService.Navigate(typeof(EventViewModel), m);
                    };
                    break;
                case MeetingStatus.Evaluating:
                    item.OnAction = async () =>
                    {
						var config = new ActionSheetConfig();

						var source = new CancellationTokenSource();
						var evaluated = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.IsEvaluated(m.Id, Settings.LoginUser.Id), source);
						if (!evaluated)
						{
							config.Add("評価する", async () =>
							{
								var result = await NavigationService.Navigate<RatingsViewModel, RatingParameter, RatingResult>(new RatingParameter() { MeetingId = m.Id, CurrentEvaluationItem = EvaluationItem.Clear });
								if (result != null)
									await this.ClosePicker(() => { }, false);

							});
						}
						else
						{
							config.Add("評価済み");
						}

						if (isOwner)
                        {
							config.Add("評価終了", async () =>
                            {
								if (await FinishEvaluationAsync(item))
								{
									item.Image = "meeting_grey";
									item.SmallIcon = "ic_trophy_variant_black_24dp";
									item.OnAction = async () => await NavigationService.Navigate(typeof(MeetingResultTabRootViewModel), m); ;

									await NavigationService.Navigate(typeof(MeetingResultTabRootViewModel), m); ;
								}
                            });
						}
						config.SetCancel();
						UserDialogs.Instance.ActionSheet(config);
					};
                    break;
                case MeetingStatus.Closed:
                    item.OnAction = async () =>
                    {
						await NavigationService.Navigate(typeof(MeetingResultTabRootViewModel), m);
                    };
                    break;
                default:
                    break;
            }

            return item;
        }

    }

    public class MeetingListItemViewModel : ListItemViewModel
    {
        public string SmallIcon
        {
            get => _smallIcon;
            set => SetProperty(ref _smallIcon, value);
        }
        private string _smallIcon;

    }
}