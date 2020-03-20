using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using TradingHatsuwa.Core.Messages;
using TradingHatsuwa.Core.ViewModels;
using TradingHatsuwa.Droid.Helpers;
using TradingHatsuwa.Droid.Views.Adapters;

namespace TradingHatsuwa.Droid.Views
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class RatingsView : BaseView<RatingsViewModel>
    {
        protected override int LayoutResource => Resource.Layout.RatingsView;
        private readonly MvxSubscriptionToken _token;

        public RatingsView()
        {
            _token = Mvx.Resolve<IMvxMessenger>().Subscribe<CloseMessage>(message =>
            {
                Mvx.Resolve<IMvxMessenger>().Unsubscribe<CloseMessage>(_token);
                Finish();
            });
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = ViewModel.Title;

            // MvxRecyclerView
            var recyclerView = FindViewById<MvxRecyclerView>(Resource.Id.ListView);
            var dividerItemDecoration = new DividerItemDecoration(recyclerView.Context, new LinearLayoutManager(this).Orientation);
            dividerItemDecoration.SetDrawable(this.GetDrawableFromResourceId(Resource.Drawable.divider));
            recyclerView.AddItemDecoration(dividerItemDecoration);
            recyclerView.Adapter = new RatingRecyclerAdapter((IMvxAndroidBindingContext)BindingContext) { HasSections = true };

        }

        /// <summary>
        /// Add Toolbar items
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.OkToolbarMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        /// <summary>
        /// Toolbar menu item selected
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.Ok:
                    ViewModel.OkCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }

}
