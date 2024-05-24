using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;

namespace ColorMC.Android;

[Service]
public class GameService : Service
{
    private const int NotificationId = 1;
    private const string ChannelId = "ColorMCGameChannel";

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel();

        var notification = new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("Foreground Service")
            .SetContentText("This is a foreground service running")
            .SetSmallIcon(Resource.Drawable.icon)
            .Build();

        StartForeground(NotificationId, notification);

        // Do heavy work on a background thread
        // StopSelf();

        new Thread(() =>
        { 
            
        });

        return StartCommandResult.NotSticky;
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, "Foreground Service Channel", NotificationImportance.Default);
            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
        }
    }
}
