using System.Collections.Generic;
using Android.Util;
using Java.IO;
using Java.Net;
using Newtonsoft.Json;
using Org.Json;
using PhotoGalleryXamarin.Extensions;
using PhotoGalleryXamarin.Models;

namespace PhotoGalleryXamarin
{
    public class FlickrFetchr
    {
        private const string ApiKey = "bb8530f0f6763bc399470ee220717fe9";
        private const string Tag = "FlickrFetchr";

        public List<GalleryItem> FetchItems(int pageIndex)
        {
            var items = new List<GalleryItem>();

            try
            {
                var url = Android.Net.Uri.Parse("https://api.flickr.com/services/rest/")
                    .BuildUpon()
                    .AppendQueryParameter("method", "flickr.photos.getRecent")
                    .AppendQueryParameter("api_key", ApiKey)
                    .AppendQueryParameter("format", "json")
                    .AppendQueryParameter("nojsoncallback", "1")
                    .AppendQueryParameter("extras", "url_s")
                    .AppendQueryParameter("page", pageIndex.ToString())
                    .Build().ToString();

                var jsonString = GetUrlString(url);
                Log.Info(Tag, $"Received JSON: {jsonString}");

                JsonTextReader reader = new JsonTextReader(new System.IO.StringReader(jsonString.ToString()));
                JsonSerializer serializer = new JsonSerializer();

                var galleryInfo = serializer.Deserialize<GalleryInfo>(reader);
                
                return galleryInfo.Gallery.GalleryItems;
            }
            catch (IOException ioe)
            {
                Log.Error(Tag, $"Failed to fetch items ", ioe);
            }
            catch (JSONException je)
            {
                Log.Error(Tag, $"Failed to parse JSON ", je);
            }

            return null;
        }

        private byte[] GetUrlBytes(string urlSpec)
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
    }
}
