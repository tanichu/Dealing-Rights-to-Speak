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
    public class EventView : BaseView<EventViewModel>
    {
        protected override int LayoutResource => Resource.Layout.EventView;

        private Button _begButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = ViewModel.Title;

            // MvxRecyclerView
            var recyclerView = FindViewById<MvxRecyclerView>(Resource.Id.ListView);
            //var dividerItemDecoration = new DividerItemDecoration(recyclerView.Context, new LinearLayoutManager(this).Orientation);
            //dividerItemDecoration.SetDrawable(GetDrawable(Resource.Drawable.divider));
            //recyclerView.AddItemDecoration(dividerItemDecoration);

            //recyclerView.HasFixedSize = false;
            recyclerView.SetLayoutManager(new GridLayoutManager(this, 3));

            var queueRecyclerView = FindViewById<MvxRecyclerView>(Resource.Id.QueueRecyclerView);
            queueRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));

            // Button
            _begButton = FindViewById<Button>(Resource.Id.BegButton);

            // event
            ViewModel.BeggedChanged += ViewModel_BeggedChanged;
        }

        private void ViewModel_BeggedChanged(object sender, EventArgs e)
        {
            SetBegButton();
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetBegButton();
        }

        private void SetBegButton()
        {
            var button = FindViewById<Button>(Resource.Id.BegButton);
            var begged = ViewModel?.Begged ?? false;
            button.SetCompoundDrawablesWithIntrinsicBounds(null, this.GetDrawableFromResourceId(begged ? Resource.Drawable.ic_action_begged_on : Resource.Drawable.ic_action_begged_off), null, null);
            button.SetTextColor(this.CreateGraphicsColor(begged ? Resource.Color.accent : Resource.Color.primaryText));
        }

        ~EventView()
        {
            ViewModel.BeggedChanged -= ViewModel_BeggedChanged;
        }
    }
}
