using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content.Res;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using PhotoGalleryXamarin.Extensions;
using PhotoGalleryXamarin.Models;
using static Android.Support.V7.Widget.RecyclerView;

namespace PhotoGalleryXamarin.Fragments
{
    public class PhotoGalleryFragment : Fragment
    {
        private new const string Tag = "PhotoGalleryFragment";

        private RecyclerView _recyclerView;
        private List<GalleryItem> _galleryItems = new List<GalleryItem>();
        private ThumbnailDownloader<PhotoHolder> _thumbnailDownloader;

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

            var responseHandler = new Handler();
            _thumbnailDownloader = new ThumbnailDownloader<PhotoHolder>(responseHandler);

            _thumbnailDownloader.OnThumbnailDownloaded = OnThumbnailDownloaded;

            _thumbnailDownloader.Start();
            var looper = _thumbnailDownloader.Looper;

            Log.Info(Tag, "Background thread started");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_photo_gallery, container, false);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.photo_recycler_view);
            _recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 3));

            SetupAdapter();

            return view;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _thumbnailDownloader.Quit();
            _thumbnailDownloader.ClearQueue();
        }

        public void SetupAdapter()
        {
            if (IsAdded)
            {
                _recyclerView.SetAdapter(new PhotoAdapter(_galleryItems, _thumbnailDownloader));
            }
        }

        private void OnThumbnailDownloaded(PhotoHolder holder, Bitmap bitmap)
        {
            var drawable = new BitmapDrawable(Resources, bitmap);
            Log.Info(Tag, "OnThumbnailDownloaded");
            holder.BindDrawable(drawable);
        }

        private void OnItemsFetched(List<GalleryItem> galleryItems)
        {
            _galleryItems = galleryItems;
            SetupAdapter();
        }

        private class PhotoAdapter : RecyclerView.Adapter
        {
            private List<GalleryItem> _galleryItems;
            private ThumbnailDownloader<PhotoHolder> _thumbnailDownloader;

            public PhotoAdapter(List<GalleryItem> galleryItems, ThumbnailDownloader<PhotoHolder> thumbnailDownloader)
            {
                _galleryItems = galleryItems;
                _thumbnailDownloader = thumbnailDownloader;
            }

            public override int ItemCount => _galleryItems.Count;

            public override ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var layoutInflater = LayoutInflater.From(parent.Context);
                var view = layoutInflater.Inflate(Resource.Layout.list_item_gallery, parent, false);

                return new PhotoHolder(view);
            }

            public override void OnBindViewHolder(ViewHolder holder, int position)
            {
                var photoHolder = (PhotoHolder)holder;
                var placeholder = holder.ItemView.GetDrawable(Resource.Drawable.bill_up_close);
                
                photoHolder.BindDrawable(placeholder);
                _thumbnailDownloader.QueueThumbnail(photoHolder, _galleryItems[position].Url);
            }
        }

        private class PhotoHolder : ViewHolder
        {
            private ImageView _itemImageView;

            public PhotoHolder(View itemView) : base(itemView)
            {
                _itemImageView = (ImageView)itemView.FindViewById(Resource.Id.item_image_view);
            }

            public void BindDrawable(Drawable resource)
            {
                _itemImageView.SetImageDrawable(resource);
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
