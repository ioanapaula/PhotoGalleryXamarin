using Android.Runtime;
using Java.IO;
using Java.Net;

namespace PhotoGalleryXamarin.Extensions
{
    public static class StreamExtensions
    {
        public static InputStream GetInputStream(this HttpURLConnection self)
        {
            return ((InputStreamInvoker)self.InputStream).BaseInputStream;
        }
    }
}
