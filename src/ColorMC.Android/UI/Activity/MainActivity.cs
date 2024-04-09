using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Android.GLRender;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Process = System.Diagnostics.Process;
using Uri = Android.Net.Uri;

namespace ColorMC.Android.UI.Activity;

[Activity(Label = "ColorMC",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    TaskAffinity = "colormc.android.game.main",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.FullUser)]
public class MainActivity : AvaloniaMainActivity<App>
{
    public static readonly Dictionary<string, GameRender> Games = [];
    public static string NativeLibDir;
    public static string CacheDir;
    public static string ExternalFilesDir;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        App.Close();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        ColorMCCore.PhoneReadFile = PhoneReadFile;
        ColorMCCore.PhoneOpenUrl = PhoneOpenUrl;
        ColorMCCore.PhoneGameLaunch = PhoneGameLaunch;

        ColorMCGui.PhoneGetSetting = PhoneGetSetting;

        CacheDir = ApplicationContext!.CacheDir!.AbsolutePath!;
        NativeLibDir = ApplicationInfo!.NativeLibraryDir!;
        ExternalFilesDir = ApplicationContext.GetExternalFilesDir(null)!.AbsolutePath;

        ColorMCAndroid.Init();

        ColorMCGui.StartPhone(GetExternalFilesDir(null)!.AbsolutePath + "/");
        PhoneConfigUtils.Init(ColorMCCore.BaseDir);

        base.OnCreate(savedInstanceState);

        ResourceUnPack.StartUnPack(this);

        BackRequested += MainActivity_BackRequested;
    }

    private void MainActivity_BackRequested(object? sender, AndroidBackRequestedEventArgs e)
    {
        if (App.AllWindow is { } window)
        {
            window.Model.BackClick();
            e.Handled = true;
        }
    }

    public Control PhoneGetSetting()
    {
        return new PhoneControl(this);
    }

    public Stream? PhoneReadFile(string file)
    {
        var uri = Uri.Parse(file);
        return ContentResolver?.OpenInputStream(uri);
    }

    protected override void OnActivityResult(int requestCode,
        [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
    }

    public void PhoneOpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }
        Uri uri = Uri.Parse(url)!;
        StartActivity(new Intent(Intent.ActionView, uri));
    }

    public void Setting()
    {
        //var mainIntent = new Intent();
        //mainIntent.SetAction("ColorMC.Minecraft.Setting");
        //StartActivity(mainIntent);
    }

    public Process PhoneGameLaunch(GameSettingObj obj, JavaInfo jvm, List<string> list,
       Dictionary<string, string> env)
    {
        var render = GameRender.RenderType.gl4es;
        ColorMCAndroid.ConfigSet(obj);
        ColorMCAndroid.ReplaceClassPath(obj, list);
        var display = AndroidHelper.GetDisplayMetrics(this);
        Bitmap bitmap;
        var image = obj.GetIconFile();
        if (File.Exists(image))
        {
            bitmap = BitmapFactory.DecodeFile(image);
        }
        else
        {
            bitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon);
        }

        var p = ColorMCAndroid.BuildRunProcess(render, display, list, obj, jvm, env, false);
        var game = new GameRender(ApplicationContext.FilesDir.AbsolutePath, obj.UUID, obj.Name,
           bitmap, p, render);
        game.GameReady += Game_GameReady;

        Games.Remove(obj.UUID);
        Games.Add(obj.UUID, game);

        game.Start();

        p.OutputDataReceived += P_OutputDataReceived;
        p.ErrorDataReceived += P_ErrorDataReceived;

        p.Exited += (a, b) =>
        {
            if (Games.Remove(obj.UUID, out var game))
            {
                game.Close();
            }
            Update();
        };

        Update();

        return p;
    }

    private void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (App.AllWindow is { } win)
            {
                if (Games.Count > 0)
                {
                    win.Model.SetChoiseContent("ColorMC", "回到游戏");
                    win.Model.SetChoiseCall("ColorMC", () =>
                    {
                        AndroidHelper.Main.Post(() =>
                        {
                            var intent = new Intent(this, typeof(GameActivity));
                            intent.PutExtra("GAME_UUID", Games.Keys.ToArray()[0]);
                            intent.AddFlags(ActivityFlags.SingleTop);
                            StartActivity(intent);
                        });
                    });
                }
                else
                {
                    win.Model.RemoveChoiseData("ColorMC");
                }
            }
        });
    }

    private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log.Error("Game Pipe", e.Data ?? "null");
    }

    private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log.Info("Game Pipe", e.Data ?? "null");
    }

    private void Game_GameReady(string uuid)
    {
        AndroidHelper.Main.Post(() =>
        {
            var intent = new Intent(this, typeof(GameActivity));
            intent.PutExtra("GAME_UUID", uuid);
            intent.AddFlags(ActivityFlags.SingleTop);
            StartActivity(intent);
        });
    }
}
