using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using Java.Lang;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Droid.Views;
using System;
using System.Collections.Generic;
using TradingHatsuwa.Core.ViewModels;
using TradingHatsuwa.Droid.Helpers;

namespace TradingHatsuwa.Droid.Views
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class UsersView : BaseView<UsersViewModel>
    {
        protected override int LayoutResource => Resource.Layout.UsersView;

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
    }
}
