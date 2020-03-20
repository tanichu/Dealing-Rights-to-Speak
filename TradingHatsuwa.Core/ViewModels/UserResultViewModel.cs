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
	public class UserResultViewModel : ListViewModel<User>
    {
        private User _user;
		private readonly IUserHubProxy _userHub;

		public UserResultViewModel(IUserHubProxy userHub)
        {
			_userHub = userHub;
		}

        public override async Task Initialize()
        {
			await CreateListItems();
        }

        public override void Prepare(User parameter)
        {
            _user = parameter;
            Title = $"{_user.Name} 個人評価結果";
        }

        private async Task CreateListItems()
        {
			try
			{
				var source = new CancellationTokenSource();
				var results = await UserDialogs.Instance.LoadingDelayedAsync(_userHub.GetResults(_user.Id), source);
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