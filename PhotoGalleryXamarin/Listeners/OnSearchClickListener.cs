using System;
using Android.Views;

namespace PhotoGalleryXamarin.Listeners
{
    public class OnSearchClickListener : Java.Lang.Object, View.IOnClickListener
    {
        public OnSearchClickListener(Action action)
        {
            OnSearchClicked = action;
        }

        public Action OnSearchClicked { get; set; }

        public void OnClick(View v)
        {
            OnSearchClicked?.Invoke();
        }
    }
}
