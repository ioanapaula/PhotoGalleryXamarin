using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace PhotoGalleryXamarin
{
    public abstract class XamarinAdapter<T> : RecyclerView.Adapter where T : RecyclerView.ViewHolder
    { 
        public abstract override int ItemCount { get; }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            OnBindViewHolderImpl(holder as T, position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return OnCreateViewHolderImpl(parent, viewType);
        }

        protected abstract T OnCreateViewHolderImpl(ViewGroup parent, int viewType);

        protected abstract void OnBindViewHolderImpl(T holder, int position);
    }
}
