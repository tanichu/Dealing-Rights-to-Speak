using Android.OS;
using MvvmCross.Binding.Droid.BindingContext;
using TradingHatsuwa.Core.ViewModels;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using Android.Runtime;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Android.Support.V7.Widget;
using TradingHatsuwa.Droid.Views.Adapters;
using TradingHatsuwa.Droid.Helpers;

namespace TradingHatsuwa.Droid.Views
{
    [MvxTabLayoutPresentation(TabLayoutResourceId = Resource.Id.tabs, ViewPagerResourceId = Resource.Id.viewpager, Title = "賞", ActivityHostViewModelType = typeof(MeetingResultTabRootViewModel))]
    [Register(nameof(MeetingResultTabAwardsView))]
    public class MeetingResultTabAwardsView : MvxFragment<MeetingResultTabAwardsViewModel>
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.MeetingResultTabAwardsView, null);

            // MvxRecyclerView
            var recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.ListView);
            var dividerItemDecoration = new DividerItemDecoration(recyclerView.Context, new LinearLayoutManager(Context).Orientation);
            dividerItemDecoration.SetDrawable(Context.GetDrawableFromResourceId(Resource.Drawable.divider));
            recyclerView.AddItemDecoration(dividerItemDecoration);
            recyclerView.Adapter = new MeetingResultTabAwardRecyclerAdapter((IMvxAndroidBindingContext)BindingContext) { HasSections = true };

            return view;
        }
    }
}
