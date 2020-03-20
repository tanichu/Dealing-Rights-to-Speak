using Acr.UserDialogs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.Data;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.Core.ViewModels
{
    public class MeetingResultTabAwardsViewModel : ListViewModel<Meeting>
    {
		private Meeting _meeting;
		private readonly IMeetingHubProxy _meetingHub;

		public MeetingResultTabAwardsViewModel(IMeetingHubProxy meetingHub)
        {
			_meetingHub = meetingHub;
		}

		public override async Task Initialize()
		{
			await CreateListItems();
		}

		public override void Prepare(Meeting parameter)
		{
			_meeting = parameter;
		}

		private async Task CreateListItems()
		{
			try
			{
				var source = new CancellationTokenSource();
				var awards = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.GetAwards(_meeting.Id), source);
				if (awards == null) return;

				foreach (var e in Enum.GetValues(typeof(EvaluationItem)))
				{
					var header = new MeetingAwardListItemViewModel()
					{
						HeaderText = $"{e.ToString()}賞",
						HeaderImage = $"evaluation_{e.ToString().ToLower()}",
						Selectable = false,
						IsSection = true,
					};

					var award = awards.SingleOrDefault(x => x.EvaluationItem == (Data.EvaluationItem)e);
					if(award != null && award.Users.Any())
					{
						foreach (var user in award.Users)
						{
							var item = new ListItemViewModel
							{

								Text = $"{user.User.Name}",
								DetailText = $"{((double)user.Score).ToString("0.0")} 点",
								Image = user.User.IconUrl(),
								Selectable = false,
							};
							header.SubItems.Add(item);
						}
					}
					else
					{
						var item = new ListItemViewModel
						{
							Text = "",
							DetailText = "評価結果はありません",
							Image = null,
							Selectable = false,
						};
						header.SubItems.Add(item);
					}
					ListItems.Add(header);
				}
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }

		}

    }
}