﻿using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PhotoGalleryXamarin.Extensions;
using PhotoGalleryXamarin.Models;
using static Android.Support.V7.Widget.RecyclerView;

namespace PhotoGalleryXamarin.Fragments
{
    public class PhotoGalleryFragment : Fragment
    {
        private new const string Tag = "PhotoGalleryFragment";
        private const int ColWidth = 300;

        private int _pageIndex;

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
            _pageIndex = 1;

            new FetchItemsTask
            {
                ResultPageIndex = _pageIndex,
                OnPostExecuteImpl = OnItemsFetched
            }.Execute();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_photo_gallery, container, false);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.photo_recycler_view);
            _recyclerView.AddGlobalLayoutListener(GlobalLayout);
            _recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 3));
            _recyclerView.ScrollChange += ScrollChanged;
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

        private void GlobalLayout() 
        {
            var columnNumber = _recyclerView.Width / ColWidth;
            _recyclerView.SetLayoutManager(new GridLayoutManager(Activity, columnNumber));
        }

        private void ScrollChanged(object sender, View.ScrollChangeEventArgs e)
        {
            if (!_recyclerView.CanScrollVertically(1))
            {
                _pageIndex++;
                new FetchItemsTask
                {
                    ResultPageIndex = _pageIndex,
                    OnPostExecuteImpl = OnItemsFetched
                }.Execute();
            }
        }

        private void OnItemsFetched(List<GalleryItem> galleryItems)
        {
            var indexToInsert = _galleryItems.Count;
            _galleryItems.AddRange(galleryItems);
            _recyclerView.GetAdapter().NotifyItemRangeInserted(indexToInsert, galleryItems.Count);
        }

        private class PhotoAdapter : XamarinAdapter<PhotoHolder>
        {
            private List<GalleryItem> _galleryItems;

            public PhotoAdapter(List<GalleryItem> galleryItems)
            {
                _galleryItems = galleryItems;
            }

            public override int ItemCount => _galleryItems.Count;

            protected override void OnBindViewHolderImpl(PhotoHolder holder, int position)
            {
                var galleryItem = _galleryItems[position];
                holder.BindGalleryItem(galleryItem);
            }

            protected override PhotoHolder OnCreateViewHolderImpl(ViewGroup parent, int viewType)
            {
                var textView = new TextView(parent.Context);

                return new PhotoHolder(textView);
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
                return new FlickrFetchr().FetchItems(ResultPageIndex);
            }
        }
    }
}
