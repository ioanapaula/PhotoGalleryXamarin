using Android.OS;
using Java.Lang;
using JavaObject = Java.Lang.Object;

namespace PhotoGalleryXamarin
{
    public abstract class XamarinAsyncTask<T> : AsyncTask<Void, Void, T>
    { 
        public System.Action<T> OnPostExecuteImpl { get; set; }

        public int ResultPageIndex { get; set; }

        protected sealed override void OnPostExecute(JavaObject result)
        {
            var unwrappedResult = (result as JavaObjectWrapper<T>).ContainedObject;

            base.OnPostExecute(unwrappedResult);

            OnPostExecuteImpl?.Invoke(unwrappedResult);
        }

        protected sealed override JavaObject DoInBackground(params JavaObject[] native_parms)
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
        public JavaObjectWrapper(T containedObject)
        {
            ContainedObject = containedObject;
        }

        public T ContainedObject { get; }
    }
}
