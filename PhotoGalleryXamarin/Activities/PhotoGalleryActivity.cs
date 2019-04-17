using Android.App;
using Android.Content;
using PhotoGalleryXamarin.Fragments;

namespace PhotoGalleryXamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class PhotoGalleryActivity : SingleFragmentActivity
    {
        public static Intent NewIntent(Context context)
        {
            return new Intent(context, typeof(PhotoGalleryActivity));
        }

        protected override Android.Support.V4.App.Fragment CreateFragment()
        {
            return PhotoGalleryFragment.NewInstance();
        }
    }
}