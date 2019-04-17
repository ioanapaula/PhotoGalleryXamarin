using System.Collections.Generic;
using Android.Net;
using Android.Util;
using Java.IO;
using Java.Net;
using Org.Json;
using PhotoGalleryXamarin.Extensions;
using PhotoGalleryXamarin.Models;

namespace PhotoGalleryXamarin
{
    public class FlickrFetchr
    {
        private const string Tag = "FlickrFetchr";
        private const string ApiKey = "bb8530f0f6763bc399470ee220717fe9";
        private const string FetchRecentsMethod = "flickr.photos.getRecent";
        private const string SearchMethod = "flickr.photos.search";

        private Uri _endpoint = Uri.Parse("https://api.flickr.com/services/rest/")
                    .BuildUpon()
                    .AppendQueryParameter("api_key", ApiKey)
                    .AppendQueryParameter("format", "json")
                    .AppendQueryParameter("nojsoncallback", "1")
                    .AppendQueryParameter("extras", "url_s")
                    .Build();

        public List<GalleryItem> FetchRecentPhotos()
        {
            string url = BuildUrl(FetchRecentsMethod, null);

            return DownloadGalleryItems(url);
        }

        public List<GalleryItem> SearchPhotos(string query)
        {
            string url = BuildUrl(SearchMethod, query);

            return DownloadGalleryItems(url);
        }

        public byte[] GetUrlBytes(string urlSpec)
        {
            var url = new URL(urlSpec);
            var connection = (HttpURLConnection)url.OpenConnection();

            try
            {
                var outputStream = new ByteArrayOutputStream();
                var inputStream = connection.GetInputStream();

                if (connection.ResponseCode != HttpStatus.Ok)
                {
                    throw new IOException($"{connection.ResponseMessage} with: {urlSpec}");
                }

                var bytesRead = 0;
                var buffer = new byte[1024];

                while ((bytesRead = inputStream.Read(buffer)) > 0)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                }

                outputStream.Close();

                return outputStream.ToByteArray();
            }
            finally
            {
                connection.Disconnect();
            }
        }

        private Java.Lang.String GetUrlString(string urlSpec)
        {
            return new Java.Lang.String(GetUrlBytes(urlSpec));
        }

        private List<GalleryItem> DownloadGalleryItems(string url)
        {
            var items = new List<GalleryItem>();

            try
            {
                var jsonString = GetUrlString(url);
                Log.Info(Tag, $"Received JSON: {jsonString}");

                var jsonBody = new JSONObject((string)jsonString);
                ParseItems(items, jsonBody);
            }
            catch (IOException ioe)
            {
                Log.Error(Tag, $"Failed to fetch items ", ioe);
            }
            catch (JSONException je)
            {
                Log.Error(Tag, $"Failed to parse JSON ", je);
            }

            return items;
        }

        private void ParseItems(List<GalleryItem> items, JSONObject jsonBody)
        {
            var photosJsonObject = jsonBody.GetJSONObject("photos");
            var photoJsonArray = photosJsonObject.GetJSONArray("photo");

            for (int i = 0; i < photoJsonArray.Length(); i++)
            {
                var photoJsonObject = photoJsonArray.GetJSONObject(i);
                var item = new GalleryItem();
                item.Id = photoJsonObject.GetString("id");
                item.Caption = photoJsonObject.GetString("title");

                if (photoJsonObject.Has("url_s"))
                {
                    item.Url = photoJsonObject.GetString("url_s");
                    items.Add(item);
                }
            }
        }

        private string BuildUrl(string method, string query)
        {
            Uri.Builder uriBuilder = _endpoint.BuildUpon()
                .AppendQueryParameter("method", method);

            if (method.Equals(SearchMethod))
            {
                uriBuilder.AppendQueryParameter("text", query);
            }

            return uriBuilder.Build().ToString();
        }
    }
}
