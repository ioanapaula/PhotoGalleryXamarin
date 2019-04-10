using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using PhotoGalleryXamarin.Models;
using static Android.Support.V7.Widget.RecyclerView;

namespace PhotoGalleryXamarin.Fragments
{
    public class PhotoGalleryFragment : Fragment
    {
        private new const string Tag = "PhotoGalleryFragment";

        private RecyclerView _recyclerView;
        private List<GalleryItem> _galleryItems = new List<GalleryItem>();

        public static PhotoGalleryFragment NewInstance()
        {
            return new PhotoGalleryFragment();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RetainInstance = true;

            new FetchItemsTask
            {
                OnPostExecuteImpl = OnItemsFetched
            }.Execute();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_photo_gallery, container, false);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.photo_recycler_view);
            _recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 3));

            SetupAdapter();

            return view;
        }

        public void SetupAdapter()
        {
            if (IsAdded)
            {
                _recyclerView.SetAdapter(new PhotoAdapter(_galleryItems));
            }
        }

        private void OnItemsFetched(List<GalleryItem> galleryItems)
        {
            _galleryItems = galleryItems;
            SetupAdapter();
        }

        private class PhotoAdapter : RecyclerView.Adapter
        {
            private List<GalleryItem> _galleryItems;

            public override int ItemCount => _galleryItems.Count;

            public PhotoAdapter(List<GalleryItem> galleryItems)
            {
                _galleryItems = galleryItems;
            }

            public override ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var textView = new TextView(parent.Context);

                return new PhotoHolder(textView);
            }

            public override void OnBindViewHolder(ViewHolder holder, int position)
            {
                var photoHolder = (PhotoHolder)holder;
                var galleryItem = _galleryItems[position];
                photoHolder.BindGalleryItem(galleryItem);
            }
        }

        private class PhotoHolder : ViewHolder
        {
            private TextView _titleTextView;

            public PhotoHolder(View itemView) : base(itemView)
            {
                _titleTextView = (TextView)itemView;
            }

            public void BindGalleryItem(GalleryItem item)
            {
                _titleTextView.Text = item.Caption;
            }
        }

        private class FetchItemsTask : XamarinAsyncTask<List<GalleryItem>>
        {
            protected override List<GalleryItem> DoInBackground()
            {
                return new FlickrFetchr().FetchItems();
            }
        }
    }
}
