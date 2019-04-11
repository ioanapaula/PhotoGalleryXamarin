using System;
using Android.Views;

namespace PhotoGalleryXamarin.Listeners
{
    public class OnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private View _view;
        private Action _action;

        public OnGlobalLayoutListener(View view, Action action)
        {
            _view = view;
            _action = action;
        }

        public void OnGlobalLayout()
        {
            _view.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
            _action?.Invoke();
        }
    }
}