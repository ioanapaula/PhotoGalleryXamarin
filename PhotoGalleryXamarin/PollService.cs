using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using PhotoGalleryXamarin.Models;

namespace PhotoGalleryXamarin
{
    [Service(Exported = true, Permission = "android.permission.BIND_JOB_SERVICE")]
    public class PollService : JobService
    {
        private const string ChannelId = "0";
        private const string Tag = "Poll Service";
        private const int JobId = 1;
        private PollTask _currentTask;

        public static void SetScheduler(Context context, bool isOn)
        {
            JobScheduler scheduler = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);

            if (isOn)
            {
                JobInfo jobInfo = new JobInfo.Builder(JobId, new ComponentName(context, Java.Lang.Class.FromType(typeof(PollService))))
                    .SetRequiredNetworkType(NetworkType.Unmetered)
                    .SetPeriodic(1000 * 60)
                    .Build();
                scheduler.Schedule(jobInfo);
            }
            else
            {
                scheduler.Cancel(JobId);
            }
        }

        public static bool IsScheduled(Context context)
        {
            JobScheduler scheduler = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);

            var isStarted = scheduler.AllPendingJobs.FirstOrDefault(jobInfo => jobInfo.Id == JobId) != null;

            return isStarted;
        }

        public override bool OnStartJob(JobParameters @params)
        {
            _currentTask = new PollTask(this);
            _currentTask.Execute(@params);
            
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            if (_currentTask != null)
            {
                _currentTask.Cancel(true);
            }

            return true;
        }

        private class PollTask : XamarinAsyncTask<JobParameters, Java.Lang.Void>
        {
            private JobService _jobService;

            public PollTask(JobService jobService)
            {
                _jobService = jobService;
            }

            protected override Java.Lang.Void DoInBackground(params JobParameters[] parameters)
            {
                var query = QueryPreferences.GetStoredQuery(_jobService);
                var lastResultId = QueryPreferences.GetLastResultId(_jobService);
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

                        var activityIntent = PhotoGalleryActivity.NewIntent(_jobService);
                        var pendingIntent = PendingIntent.GetActivity(_jobService, 0, activityIntent, 0);
                        var resources = _jobService.Resources;

                        var notification = new NotificationCompat.Builder(_jobService, ChannelId)
                            .SetTicker(resources.GetString(Resource.String.new_pictures_title))
                            .SetSmallIcon(Android.Resource.Drawable.IcMenuReportImage)
                            .SetContentTitle(resources.GetString(Resource.String.new_pictures_title))
                            .SetContentText(resources.GetString(Resource.String.new_pictures_text))
                            .SetContentIntent(pendingIntent)
                            .SetAutoCancel(true)
                            .Build();

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var name = resources.GetString(Resource.String.channel_name);
                            var importance = NotificationImportance.Default;
                            var channel = new NotificationChannel(ChannelId, name, importance);
                            var notificationManager = NotificationManager.FromContext(_jobService);

                            notificationManager.CreateNotificationChannel(channel);
                        }

                        NotificationManagerCompat.From(_jobService).Notify(0, notification);
                    }

                    QueryPreferences.SetLastResultId(_jobService, resultId);
                }

                var jobParams = parameters?.Any() == true ? parameters[0] : null;
                _jobService.JobFinished(jobParams, false);

                return null;
            }
        }
    }   
}
