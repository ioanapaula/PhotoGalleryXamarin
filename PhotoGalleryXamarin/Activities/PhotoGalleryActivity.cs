using Android.App;
using PhotoGalleryXamarin.Fragments;

namespace PhotoGalleryXamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class PhotoGalleryActivity : SingleFragmentActivity
    {
        protected override Android.Support.V4.App.Fragment CreateFragment()
        {
            return PhotoGalleryFragment.NewInstance();
        }
    }
}