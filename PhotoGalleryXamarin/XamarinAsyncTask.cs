using Android.OS;
using Java.Lang;

namespace PhotoGalleryXamarin
{
    public abstract class XamarinAsyncTask<T> : AsyncTask<Void, Void, T>
    {
        protected sealed override void OnPostExecute(Object result)
        {
            var unwrappedResult = (result as JavaObjectWrapper<T>).ContainedObject;

            base.OnPostExecute(unwrappedResult);

            OnPostExecuteImpl?.Invoke(unwrappedResult);
        }

        public System.Action<T> OnPostExecuteImpl { get; set; }

        protected sealed override Object DoInBackground(params Object[] native_parms)
        {
            return new JavaObjectWrapper<T>(DoInBackground());
        }

        protected abstract T DoInBackground();

        protected override T RunInBackground(params Void[] @params)
        {
            return default(T);
        }
    }

    public class JavaObjectWrapper<T> : Java.Lang.Object
    {
        public T ContainedObject { get; }

        public JavaObjectWrapper(T containedObject)
        {
            ContainedObject = containedObject;
        }
    }
}
