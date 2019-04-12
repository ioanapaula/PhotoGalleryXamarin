using System;
using Android.Views;
using PhotoGalleryXamarin.Listeners;

namespace PhotoGalleryXamarin.Extensions
{
    public static class ViewExtensions
    {
        public static void AddGlobalLayoutListener(this View self, Action action)
        {
            self.ViewTreeObserver.AddOnGlobalLayoutListener(new OnGlobalLayoutListener(self, action));
        }
    }
}