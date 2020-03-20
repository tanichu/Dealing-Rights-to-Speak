using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TradingHatsuwa.Droid.Helpers
{
    public static class Extensions
    {
        public static Android.Graphics.Color CreateGraphicsColor(this Context context, int resourceId)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                return new Android.Graphics.Color(context.GetColor(resourceId));
            }
            else
            {
#pragma warning disable CS0618 // 型またはメンバーが古い形式です
                return new Android.Graphics.Color(context.Resources.GetColor(resourceId));
#pragma warning restore CS0618 // 型またはメンバーが古い形式です
            }
        }

        public static Drawable GetDrawableFromResourceId(this Context context, int resourceId)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                return context.GetDrawable(resourceId);
            }
            else
            {
#pragma warning disable CS0618 // 型またはメンバーが古い形式です
                return context.Resources.GetDrawable(resourceId);
#pragma warning restore CS0618 // 型またはメンバーが古い形式です
            }
        }

    }
}