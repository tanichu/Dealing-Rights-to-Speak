using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Droid.Views.Attributes;
using TradingHatsuwa.Core.ViewModels;

namespace TradingHatsuwa.Droid.Views
{
    [MvxActivityPresentation]
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MeetingResultTabRootView : BaseView<MeetingResultTabRootViewModel>
    {
        protected override int LayoutResource => Resource.Layout.MeetingResultTabRootView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = ViewModel.Title;

            if (bundle == null)
            {
                ViewModel.ShowInitialViewModelsCommand.Execute();
            }
        }
    }
}
