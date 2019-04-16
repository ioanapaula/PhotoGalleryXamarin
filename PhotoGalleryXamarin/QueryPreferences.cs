using System;
using Android.Content;
using Android.Preferences;

namespace PhotoGalleryXamarin
{
    public class QueryPreferences
    {
        public const string PrefSearchQuery = "searchQuery";

        public static String GetStoredQuery(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context).GetString(PrefSearchQuery, null);
        }

        public static void SetStoredQuery(Context context, string query)
        {
            PreferenceManager.GetDefaultSharedPreferences(context)
                .Edit()
                .PutString(PrefSearchQuery, query)
                .Apply();
        }
    }
}
