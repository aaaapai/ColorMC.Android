using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Android.GameButton;
using ColorMC.Android.GLRender;
using ColorMC.Core;
using ColorMC.Gui;
using System.IO;
using System.Linq;
using Uri = Android.Net.Uri;

namespace ColorMC.Android.UI.Activity;

[Activity(Label = "ColorMC",
    Icon = "@drawable/icon",
    MainLauncher = true,
    TaskAffinity = "colormc.android.game.main",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.FullUser)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override void OnDestroy()
    {
        base.OnDestroy();

        App.Close();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        ColorMCCore.PhoneReadFile = PhoneReadFile;
        ColorMCCore.PhoneOpenUrl = PhoneOpenUrl;

        ColorMCGui.PhoneGetSetting = PhoneGetSetting;

        ColorMCAndroid.Activity = this;

        ColorMCAndroid.CacheDir = ApplicationContext!.CacheDir!.AbsolutePath!;
        ColorMCAndroid.NativeLibDir = ApplicationInfo!.NativeLibraryDir!;
        ColorMCAndroid.ExternalFilesDir = GetExternalFilesDir(null)!.AbsolutePath;
        ColorMCAndroid.FilesDir = ApplicationContext.FilesDir!.AbsolutePath!;

        ColorMCAndroid.Init();
        ButtonManage.Load();

        ColorMCGui.StartPhone(ColorMCAndroid.ExternalFilesDir + "/");
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

    public void GameSetting()
    {
        var intent = new Intent(this, typeof(GameActivity));
        intent.PutExtra("EDIT_LAYOUT", true);
        intent.AddFlags(ActivityFlags.SingleTop);
        StartActivity(intent);
    }

    public void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (App.AllWindow is { } win)
            {
                if (ColorMCAndroid.Games.Count > 0)
                {
                    win.Model.SetChoiseContent("ColorMC", "回到游戏");
                    win.Model.SetChoiseCall("ColorMC", () =>
                    {
                        AndroidHelper.Main.Post(() =>
                        {
                            var intent = new Intent(this, typeof(GameActivity));
                            intent.PutExtra("GAME_UUID", ColorMCAndroid.Games.Keys.ToArray()[0]);
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
}
