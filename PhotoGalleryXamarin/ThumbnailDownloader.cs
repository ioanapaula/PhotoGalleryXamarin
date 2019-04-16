using System;
using System.Collections.Concurrent;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Java.IO;

namespace PhotoGalleryXamarin
{
    public class ThumbnailDownloader<T> : HandlerThread where T : Java.Lang.Object
    {
        private const string Tag = "ThumbnailDownloader";
        private const int MessageDownload = 0;

        private bool _hasQuit = false;
        private Handler _requestHandler;
        private Handler _responseHandler;
        private ConcurrentDictionary<T, string> _requestMap = new ConcurrentDictionary<T, string>();

        public ThumbnailDownloader(Handler responseHandler) : base(Tag)
        {
            _responseHandler = responseHandler;
        }

        public Action<T, Bitmap> OnThumbnailDownloaded { get; set; }

        public void SetThumbnailDownloadListener(Action<T, Bitmap> action)
        {
            OnThumbnailDownloaded = action;
        }

        public void QueueThumbnail(T target, string url)
        {
            Log.Info(Tag, $"Got a URL: {url}");

            if (url == null)
            {
                _requestMap.TryRemove(target, out _);
            }
            else
            {
                _requestMap.TryAdd(target, url);
                _requestHandler.ObtainMessage(MessageDownload, target).SendToTarget();
            }
        }

        public void ClearQueue()
        {
            _requestHandler.RemoveMessages(MessageDownload);
            _requestMap.Clear();
            Log.Info(Tag, "Cleared queue");
        }

        public override bool Quit()
        {
            _hasQuit = true;
            return base.Quit();
        }

        protected override void OnLooperPrepared()
        {
            _requestHandler = new Handler(HandleAction);
        }

        private void HandleAction(Message msg)
        {
            if (msg.What == MessageDownload)
            {
                T target = (T)msg.Obj;
                _requestMap.TryGetValue(target, out string value);
                Log.Info(Tag, $"Got a request for URL: {value}");
                HandleRequest(target);
            }
        }

        private void HandleRequest(T target)
        {
            try
            {
                Bitmap bitmap = null;
                _requestMap.TryGetValue(target, out string url);

                if (url != null)
                {
                    var bitmapBytes = new FlickrFetchr().GetUrlBytes(url);
                    bitmap = BitmapFactory.DecodeByteArray(bitmapBytes, 0, bitmapBytes.Length);
                    Log.Info(Tag, $"Bitmap created from {url}");
                }

                _responseHandler.Post(() => 
                {
                    _requestMap.TryGetValue(target, out string bitmapUrl);

                    if (bitmapUrl != url || _hasQuit)
                    {
                        return;
                    }

                    _requestMap.TryRemove(target, out _);
                    OnThumbnailDownloaded?.Invoke(target, bitmap);
                });
            }
            catch (IOException ioe)
            {
                Log.Error(Tag, "Error downloading image", ioe);
            }
        }
    }
}
