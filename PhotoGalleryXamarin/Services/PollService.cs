using System;
﻿using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Java.Util.Concurrent;
using PhotoGalleryXamarin.Models;

namespace PhotoGalleryXamarin.Services
{
    [Service]
    public class PollService : IntentService
    {
        private const string Tag = "PollService";
        private const string ChannelId = "0";

        public PollService() : base(Tag)
        {
        }

        public static void SetServiceAlarm(Context context, bool isOn)
        {
            var intent = NewIntent(context);
            var pendingIntent = PendingIntent.GetService(context, 0, intent, 0);
            var pollIntervalMs = TimeUnit.Minutes.ToMillis(15);

            var alarmManager = (AlarmManager)context.GetSystemService(AlarmService);

            if (isOn)
            {
                alarmManager.SetRepeating(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime(), pollIntervalMs, pendingIntent);
            }
            else
            {
                alarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
            }
        }

        public static bool IsServiceAlarmOn(Context context)
        {
            var intent = NewIntent(context);
            var pendingIntent = PendingIntent.GetService(context, 0, intent, PendingIntentFlags.NoCreate);

            return pendingIntent != null;
        }

        public static Intent NewIntent(Context context)
        {
            return new Intent(context, typeof(PollService));
        }

        protected override void OnHandleIntent(Intent intent)
        {
            if (!IsNetworkAvailableAndConnected())
            {
                return;
            }

            var query = QueryPreferences.GetStoredQuery(this);
            var lastResultId = QueryPreferences.GetLastResultId(this);
            List<GalleryItem> items;

            if (query == null)
            {
                items = new FlickrFetchr().FetchRecentPhotos();
            }
            else
            {
                items = new FlickrFetchr().SearchPhotos(query);
            }

            if (items.Count != 0)
            {
                var resultId = items[0].Id;
                if (resultId.Equals(lastResultId))
                {
                    Log.Info(Tag, $"Got an old result: {resultId}");
                }
                else
                {
                    Log.Info(Tag, $"Got a new result: {resultId}");

                    var activityIntent = PhotoGalleryActivity.NewIntent(this);
                    var pendingIntent = PendingIntent.GetActivity(this, 0, activityIntent, 0);

                    var notification = new NotificationCompat.Builder(this, ChannelId)
                        .SetTicker(Resources.GetString(Resource.String.new_pictures_title))
                        .SetSmallIcon(Android.Resource.Drawable.IcMenuReportImage)
                        .SetContentTitle(Resources.GetString(Resource.String.new_pictures_title))
                        .SetContentText(Resources.GetString(Resource.String.new_pictures_text))
                        .SetContentIntent(pendingIntent)
                        .SetAutoCancel(true)
                        .Build();
                        
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        var name = GetString(Resource.String.channel_name);
                        var description = GetString(Resource.String.channel_description);
                        var importance = NotificationImportance.Default;
                        var channel = new NotificationChannel(ChannelId, name, importance);
                        var notificationManager = NotificationManager.FromContext(this);

                        notificationManager.CreateNotificationChannel(channel);
                    }

                    NotificationManagerCompat.From(this).Notify(0, notification);
                }

                QueryPreferences.SetLastResultId(this, resultId);
            }
        }

        private bool IsNetworkAvailableAndConnected()
        {
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var isNetworkAvailable = connectivityManager.ActiveNetworkInfo != null;
            var isNetworkConnected = isNetworkAvailable && connectivityManager.ActiveNetworkInfo.IsConnected;

            return isNetworkConnected;
        }
    }
}
