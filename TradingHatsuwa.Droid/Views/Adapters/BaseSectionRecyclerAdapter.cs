using Android.Support.V7.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TradingHatsuwa.Core.ViewModels;

namespace TradingHatsuwa.Droid.Views.Adapters
{
    public abstract class BaseSectionRecyclerAdapter : MvxRecyclerAdapter
    {
        public bool HasSections { get; set; }

        public BaseSectionRecyclerAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext)
        {
        }

        protected override void SetItemsSource(IEnumerable value)
        {
            if (HasSections)
            {
                if (value == null)
                {
                    // 戻る画面遷移時、value == null で呼ばれる
                    //if (ItemsSource != null)
                    //{
                    //    foreach (ListItemViewModel item in ItemsSource)
                    //    {
                    //        item.SubItems.CollectionChanged -= SubItems_CollectionChanged;
                    //    }
                    //}
                }
                else
                {
                    // 途中でセクションを追加・削除しなければ下記の考慮不要
                    ///// <class>ListItemViewModel</class> の子要素コレクションに変化があった場合、OnItemsSourceCollectionChanged を発火させる
                    //((ObservableCollection<ListItemViewModel>)value).CollectionChanged += (o, e) =>
                    //{
                    //    if (e.NewItems != null)
                    //    {
                    //        foreach (ListItemViewModel item in e.NewItems)
                    //        {
                    //            item.SubItems.CollectionChanged += SubItems_CollectionChanged;
                    //        }
                    //    }
                    //    if (e.OldItems != null)
                    //    {
                    //        foreach (ListItemViewModel item in e.OldItems)
                    //        {
                    //            item.SubItems.CollectionChanged -= SubItems_CollectionChanged;
                    //        }
                    //    }
                    //};
                    foreach (ListItemViewModel item in value)
                    {
                        item.SubItems.CollectionChanged += (o, e) =>
                        {
                            switch (e.Action)
                            {
                                case NotifyCollectionChangedAction.Add:
                                    OnItemsSourceCollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, CountItems(item) + e.NewStartingIndex));
                                    break;
                                case NotifyCollectionChangedAction.Remove:
                                    OnItemsSourceCollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, CountItems(item) + e.OldStartingIndex));
                                    break;
                                //case NotifyCollectionChangedAction.Replace:
                                //    break;
                                //case NotifyCollectionChangedAction.Move:
                                //    break;
                                //case NotifyCollectionChangedAction.Reset:
                                //    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        };
                    }
                }
            }

            base.SetItemsSource(value);

            int CountItems(ListItemViewModel section)
            {
                var count = 1; // sender section

                // 対象の section の前にあるセクション数＋サブアイテム数
                var index = ListItems.IndexOf(section);
                for (int i = 0; i < index; i++)
                {
                    count++; // section 
                    count += ListItems[i].SubItems.Count; // subitems
                }

                return count;
            }
        }

        //private void SubItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    OnItemsSourceCollectionChanged(sender, e);
        //}

        /// <summary>
        /// <code>ItemsSource</code> を ListItemViewModel 型に変換
        /// </summary>
        private List<ListItemViewModel> ListItems
        {
            get
            {
                var list = new List<ListItemViewModel>();
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        list.Add((ListItemViewModel)item);
                    }
                }
                return list;
            }
        }

        public override int ItemCount => HasSections ? ListItems.Count + ListItems.SelectMany(i => i.SubItems).Count() : base.ItemCount;

        public override object GetItem(int viewPosition)
        {
            if (HasSections)
            {
                var list = new List<ListItemViewModel>();
                foreach (var item in ListItems)
                {
                    list.Add(item);
                    list.AddRange(item.SubItems);
                }

                return list.Count > viewPosition ? list[viewPosition] : null;
            }
            else
            {
                return ListItems[viewPosition];
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);

            if (HasSections)
            {
                var item = (ListItemViewModel)GetItem(position);
                if (item != null)
                    holder.ItemView.Enabled = !item.IsSection && item.Selectable;
            }
            else
            {
                holder.ItemView.Enabled = ListItems[position].Selectable;
            }

        }

        
    }
}