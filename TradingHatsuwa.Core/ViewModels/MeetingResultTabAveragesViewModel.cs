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
	public class MeetingResultTabAveragesViewModel : ListViewModel<Meeting>
    {
		private Meeting _meeting;
		private readonly IMeetingHubProxy _meetingHub;

		public MeetingResultTabAveragesViewModel(IMeetingHubProxy meetingHub)
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
				var results = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.GetResults(_meeting.Id), source);
				if (results == null) return;

				foreach (var e in Enum.GetValues(typeof(EvaluationItem)))
				{
					var score = results.SingleOrDefault(x => x.EvaluationItem == (Data.EvaluationItem)e).Score;
					var item = new ListItemViewModel
					{
						Text = e.ToString(),
						DetailText = (score == null) ? "評価結果はありません" : $"{((double)score).ToString("0.0")} 点",
						Image = $"evaluation_{e.ToString().ToLower()}",
						Selectable = false,
					};
					ListItems.Add(item);
				}
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}


    }
}