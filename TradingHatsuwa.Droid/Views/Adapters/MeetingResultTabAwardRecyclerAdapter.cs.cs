using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using TradingHatsuwa.Core.ViewModels;

namespace TradingHatsuwa.Droid.Views.Adapters
{
	public class MeetingResultTabAwardRecyclerAdapter : BaseSectionRecyclerAdapter
	{
		public override IMvxTemplateSelector ItemTemplateSelector => new ListItemTemplateSelector();

		public MeetingResultTabAwardRecyclerAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext)
        {
		}

		private class ListItemTemplateSelector : MvxTemplateSelector<ListItemViewModel>
		{
			public override int GetItemLayoutId(int fromViewType)
			{
				return fromViewType == 1 ?
					Resource.Layout.ListItem_MeetingAwardHeader :
					Resource.Layout.ListItem_MeetingAward;
			}
			protected override int SelectItemViewType(ListItemViewModel forItemObject)
			{
				return forItemObject.IsSection ? 1 : 0;
			}
		}
	}
}