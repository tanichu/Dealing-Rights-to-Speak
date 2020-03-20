using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using TradingHatsuwa.Data;

namespace TradingHatsuwa.Core.ViewModels
{
    public class MeetingResultTabRootViewModel : BaseViewModel<Meeting>
	{
		private Meeting _meeting;

		public MeetingResultTabRootViewModel()
        {
        }

		public override void Prepare(Meeting parameter)
		{
			_meeting = parameter;
			Title = _meeting.Name;
		}

		public IMvxAsyncCommand ShowInitialViewModelsCommand => new MvxAsyncCommand(async () =>
        {
            var tasks = new List<Task>();
			//tasks.Add(NavigationService.Navigate<MeetingResultTabAwardsViewModel>());
			//tasks.Add(NavigationService.Navigate<MeetingResultTabAveragesViewModel>());
			tasks.Add(NavigationService.Navigate(typeof(MeetingResultTabAwardsViewModel), _meeting));
			tasks.Add(NavigationService.Navigate(typeof(MeetingResultTabAveragesViewModel), _meeting));
			await Task.WhenAll(tasks);
        });

    }
}