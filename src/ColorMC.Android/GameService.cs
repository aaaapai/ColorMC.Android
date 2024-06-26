using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Systems;
using Android.Util;
using AndroidX.Core.App;
using Java.Lang;

namespace ColorMC.Android;

public static partial class GameNative
{
    [LibraryImport("libcolormcnative_launch.so", EntryPoint = "start",
        StringMarshalling = StringMarshalling.Utf8)]
    public static partial int Start(int size, IntPtr path);

    public static unsafe void Start(string[] arg)
    {
        Log.Info("ColorMC", $"Game start :{arg}");
        int size = arg.Length;
        IntPtr[] ptrArray = new IntPtr[size];

        for (int i = 0; i < size; i++)
        {
            // 将每个字符串转换为非托管字符串指针
            ptrArray[i] = Marshal.StringToHGlobalAnsi(arg[i]);
        }

        // 分配非托管内存来存储指针数组
        IntPtr intPtr = Marshal.AllocHGlobal(ptrArray.Length * IntPtr.Size);
        Marshal.Copy(ptrArray, 0, intPtr, ptrArray.Length);

        int res = Start(arg.Length, intPtr);
        Log.Info("ColorMC", $"Game exit :{res}");

        // 获取指针数组
        ptrArray = new IntPtr[size];
        Marshal.Copy(intPtr, ptrArray, 0, size);

        // 释放每个字符串的非托管内存
        for (int i = 0; i < size; i++)
        {
            Marshal.FreeHGlobal(ptrArray[i]);
        }

        // 释放指针数组的非托管内存
        Marshal.FreeHGlobal(intPtr);
    }
}

[Service(Exported = true, IsolatedProcess = true, ForegroundServiceType = ForegroundService.TypeMediaProjection,
    Name = "com.coloryr.colormc.android.GameForegroundService")]
public class GameService : Service
{
    private const int NotificationId = 1;
    private const string ChannelId = "ColorMCGameChannel";

    public override void OnCreate()
    {
        base.OnCreate();
        // 创建一个通知渠道 (Android 8.0 及以上需要)
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, "My Service Channel", NotificationImportance.Default);
            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
        }
    }

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        Log.Info("ColorMC", $"Game service start");

        var notification = new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("Foreground Service")
            .SetContentText("This is a foreground service running")
            .SetSmallIcon(Resource.Drawable.icon)
            .Build();

        StartForeground(NotificationId, notification);

        // Do heavy work on a background thread
        // StopSelf();

        var args = intent.GetStringArrayExtra("Args");
        var keys = intent.GetStringArrayExtra("Keys");
        var values = intent.GetStringArrayExtra("Values");

        for (int a = 0; a < keys.Length; a++)
        {
            Os.Setenv(keys[a], values[a], true);
        }

        new Thread(() =>
        {
            GameNative.Start(args);
        }).Start();

        return StartCommandResult.NotSticky;
    }
}
