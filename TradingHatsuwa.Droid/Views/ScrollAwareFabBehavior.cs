using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Util;

namespace TradingHatsuwa.Droid.Views
{
    public class ScrollAwareFabBehavior : CoordinatorLayout.Behavior
    {
        public ScrollAwareFabBehavior(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public override bool LayoutDependsOn(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
        {
            var d = dependency as AppBarLayout;
            if (d != null)
            {
                var fab = child.JavaCast<FloatingActionButton>();
                var listener = new FabOffsetter(parent, fab);
                if (!(bool)(bool?)d.GetTag(Resource.Id.ScrollAwareFabBehavior_AddedOnOffsetChangedListener))
                {
                    d.SetTag(Resource.Id.ScrollAwareFabBehavior_AddedOnOffsetChangedListener, true);
                    d.AddOnOffsetChangedListener(listener);
                }
            }
            return d != null || base.LayoutDependsOn(parent, child, dependency);
        }

        public override bool OnDependentViewChanged(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
        {
            var d = dependency as AppBarLayout;
            return d != null || base.OnDependentViewChanged(parent, child, dependency);
        }
    }
}