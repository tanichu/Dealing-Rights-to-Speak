using Android.Support.Design.Widget;

namespace TradingHatsuwa.Droid.Views
{
    public class FabOffsetter : Java.Lang.Object, AppBarLayout.IOnOffsetChangedListener
    {
        private readonly CoordinatorLayout _parent;
        private readonly FloatingActionButton _fab;

        public FabOffsetter(CoordinatorLayout parent, FloatingActionButton child)
        {
            _parent = parent;
            _fab = child;
        }

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            // fab should scroll out down in sync with the appBarLayout scrolling out up.
            // let's see how far along the way the appBarLayout is
            // (if displacementFraction == 0.0f then no displacement, appBar is fully expanded;
            //  if displacementFraction == 1.0f then full displacement, appBar is totally collapsed)
            float displacementFraction = -verticalOffset / (float)appBarLayout.Height;

            // need to separate translationY on the fab that comes from this behavior
            // and one that comes from other sources
            // translationY from this behavior is stored in a tag on the fab
            float translationYFromThis = (float)(float?)_fab.GetTag(Resource.Id.fab_translationY_from_AppBarBoundFabBehavior);

            // top position, accounting for translation not coming from this behavior
            float topUntranslatedFromThis = _fab.Top + _fab.TranslationY - translationYFromThis;

            // total length to displace by (from position uninfluenced by this behavior) for a full appBar collapse
            float fullDisplacement = _parent.Bottom - topUntranslatedFromThis;

            // calculate and store new value for displacement coming from this behavior
            float newTranslationYFromThis = fullDisplacement * displacementFraction;
            _fab.SetTag(Resource.Id.fab_translationY_from_AppBarBoundFabBehavior, newTranslationYFromThis);

            // update translation value by difference found in this step
            _fab.TranslationY = newTranslationYFromThis - translationYFromThis + _fab.TranslationY;
        }
    }

}