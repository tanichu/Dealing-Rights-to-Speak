using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using TradingHatsuwa.Core.ViewModels;

namespace TradingHatsuwa.Droid.Views.Adapters
{
    public class SampleRecyclerAdapter : BaseSectionRecyclerAdapter
    {
        public override IMvxTemplateSelector ItemTemplateSelector => new ListItemTemplateSelector();

        public SampleRecyclerAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext)
        {
        }

        private class ListItemTemplateSelector : MvxTemplateSelector<ListItemViewModel>
        {
            public override int GetItemLayoutId(int fromViewType)
            {
                return fromViewType == 1 ?
                    Resource.Layout.ListItem_User :
                    Resource.Layout.ListItem_User;
            }
            protected override int SelectItemViewType(ListItemViewModel forItemObject)
            {
                return forItemObject.IsSection ? 1 : 0;
            }
        }
    }    
}