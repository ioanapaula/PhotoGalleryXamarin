using System;
using Android.Graphics.Drawables;
using Android.Support.V4.Content.Res;
using Android.Views;
using static Android.Support.V7.Widget.RecyclerView;

namespace PhotoGalleryXamarin.Extensions
{
    public static class ViewExtensions
    {
        public static Drawable GetDrawable(this View self, int resourceId)
        {
            return ResourcesCompat.GetDrawable(self.Resources, resourceId, self.Context.Theme);
        }
    }
}
