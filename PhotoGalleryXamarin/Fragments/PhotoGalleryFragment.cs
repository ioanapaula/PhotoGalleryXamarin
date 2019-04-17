using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using PhotoGalleryXamarin.Extensions;
using PhotoGalleryXamarin.Listeners;
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

        private Android.Support.V7.Widget.SearchView _searchView;
        private IMenuItem _searchItem;
        private ProgressBar _progressBar;

        public static PhotoGalleryFragment NewInstance()
        {
            return new PhotoGalleryFragment();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RetainInstance = true;
            HasOptionsMenu = true;

            UpdateItems();

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
            _progressBar = (ProgressBar)view.FindViewById(Resource.Id.indeterminateBar);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.photo_recycler_view);
            _recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 3));

            SetupAdapter();

            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);

            inflater.Inflate(Resource.Menu.fragment_photo_gallery, menu);

            _searchItem = menu.FindItem(Resource.Id.menu_item_search);
            _searchView = (Android.Support.V7.Widget.SearchView)_searchItem.ActionView;

            _searchView.QueryTextSubmit += QueryTextSubmitted;
            _searchView.QueryTextChange += QueryTextChanged;
            _searchView.SetOnSearchClickListener(new OnSearchClickListener(SearchViewClicked));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_item_clear:
                    QueryPreferences.SetStoredQuery(Activity, null);
                    UpdateItems();

                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
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

        private void SearchViewClicked()
        {
            var query = QueryPreferences.GetStoredQuery(Activity);
            _searchView.SetQuery(query, false);
        }

        private void QueryTextChanged(object sender, Android.Support.V7.Widget.SearchView.QueryTextChangeEventArgs e)
        {
            Log.Debug(Tag, $"QueryTextChange: {e.NewText}");
        }

        private void QueryTextSubmitted(object sender, Android.Support.V7.Widget.SearchView.QueryTextSubmitEventArgs e)
        {
            Log.Debug(Tag, $"QueryTextSubmit: {e.Query}");
            QueryPreferences.SetStoredQuery(Activity, e.Query);
            HideKeyboard(Context, _searchView);
            _searchItem.CollapseActionView();
            
            UpdateItems();
        }

        private void HideKeyboard(Context context, View view)
        {
            var inputMethodManager = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(view.WindowToken, 0);
        }

        private void SetProgressVisibility(bool isVisible)
        {
            if (isVisible)
            {
                _recyclerView.Visibility = ViewStates.Gone;
                _progressBar.Visibility = ViewStates.Visible;
            }
            else
            {
                _recyclerView.Visibility = ViewStates.Visible;
                _progressBar.Visibility = ViewStates.Gone;
            }
        }

        private void UpdateItems()
        {
            var query = QueryPreferences.GetStoredQuery(Activity);

            new FetchItemsTask(query)
            {
                OnPreExecuteImpl = OnFetchStarted,
                OnPostExecuteImpl = OnItemsFetched
            }.Execute();
        }

        private void OnThumbnailDownloaded(PhotoHolder holder, Bitmap bitmap)
        {
            var drawable = new BitmapDrawable(Resources, bitmap);
            Log.Info(Tag, "OnThumbnailDownloaded");
            holder.BindDrawable(drawable);
        }

        private void OnItemsFetched(List<GalleryItem> galleryItems)
        {
            if (_progressBar != null && _recyclerView != null)
            {
                SetProgressVisibility(false);
            }

            _galleryItems = galleryItems;
            SetupAdapter();
        }

        private void OnFetchStarted()
        {
            if (_progressBar != null && _recyclerView != null)
            {
                SetProgressVisibility(true);
            }
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
            private string _query;

            public FetchItemsTask(string query)
            {
                _query = query;
            }

            protected override List<GalleryItem> DoInBackground()
            {
                if (_query == null)
                {
                    return new FlickrFetchr().FetchRecentPhotos();
                }
                else
                {
                    return new FlickrFetchr().SearchPhotos(_query);
                }
            }
        }
    }
}
