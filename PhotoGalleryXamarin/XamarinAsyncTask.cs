using System.Linq;
using Android.OS;
using Java.Lang;
using JavaObject = Java.Lang.Object;

namespace PhotoGalleryXamarin
{
    public abstract class XamarinAsyncTask<T1, T2> : AsyncTask<T1, Void, T2> where T1 : JavaObject
    {
        public System.Action<T2> OnPostExecuteImpl { get; set; }

        protected sealed override void OnPostExecute(JavaObject result)
        {
            var unwrappedResult = (result as JavaObjectWrapper<T2>).ContainedObject;

            base.OnPostExecute(unwrappedResult);

            OnPostExecuteImpl?.Invoke(unwrappedResult);
        }

        protected sealed override JavaObject DoInBackground(params JavaObject[] native_parms)
        {
            if (native_parms.Any())
            {
                return new JavaObjectWrapper<T2>(DoInBackground(native_parms.First() as T1));
            }

            return new JavaObjectWrapper<T2>(DoInBackground());
        }

        protected abstract T2 DoInBackground(params T1[] parameters);

        protected override T2 RunInBackground(params T1[] @params)
        {
            return default(T2);
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
