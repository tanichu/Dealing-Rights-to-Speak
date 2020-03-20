using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using MvvmCross.Droid.Support.V7.RecyclerView;
using TradingHatsuwa.Core.ViewModels;
using TradingHatsuwa.Droid.Helpers;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

namespace TradingHatsuwa.Droid.Views
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MenuView : BaseView<MenuViewModel>
    {
        protected override int LayoutResource => Resource.Layout.MenuView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = ViewModel.Title;

            // MvxRecyclerView
            var recyclerView = FindViewById<MvxRecyclerView>(Resource.Id.ListView);
            var dividerItemDecoration = new DividerItemDecoration(recyclerView.Context, new LinearLayoutManager(this).Orientation);
            dividerItemDecoration.SetDrawable(this.GetDrawableFromResourceId(Resource.Drawable.divider));
            recyclerView.AddItemDecoration(dividerItemDecoration);
        }

        public override async void OnBackPressed()
        {
            if (Profile.CurrentProfile != null)
            {
                var result = await ViewModel.ConfirmLogoutAsync();
                if (result == false) return; // cancel

                LoginManager.Instance.LogOut();
            }

            Finish();
        }

    }

}
