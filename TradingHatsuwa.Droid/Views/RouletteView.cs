using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using TradingHatsuwa.Core.ViewModels;

namespace TradingHatsuwa.Droid.Views
{
    [MvxDialogFragmentPresentation]
    [Register(nameof(RouletteView))]
    public class RouletteView : MvxDialogFragment<RouletteViewModel>
    {
        public RouletteView()
        {
        }

        protected RouletteView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.RouletteView, null);

            return view;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var dialog = base.OnCreateDialog(savedInstanceState);

            // request a window without the title
            dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            this.Cancelable = false;
            return dialog;
        }

    }
}