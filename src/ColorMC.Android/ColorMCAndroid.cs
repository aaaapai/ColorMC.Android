using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Systems;
using Android.Util;
using ColorMC.Android.components;
using ColorMC.Android.GLRender;
using ColorMC.Android.UI.Activity;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui;
using ColorMC.Gui.Objs;
using Java.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Path = System.IO.Path;
using Process = System.Diagnostics.Process;

namespace ColorMC.Android;

public static class ColorMCAndroid
{
    public static MainActivity Activity { get; set; }
    public static GameActivity Game { get; set; }

    public static readonly Dictionary<string, GameRender> Games = [];
    public static string NativeLibDir;
    public static string CacheDir;
    public static string ExternalFilesDir;
    public static string FilesDir;

    public static void Init()
    {
        ColorMCCore.PhoneJvmInstall = PhoneJvmInstall;
        ColorMCCore.PhoneStartJvm = PhoneStartJvm;
        ColorMCCore.PhoneGetDataDir = PhoneGetDataDir;
        ColorMCCore.PhoneJvmRun = PhoneJvmRun;
        ColorMCCore.PhoneGameLaunch = PhoneGameLaunch;

        ColorMCGui.PhoneGetFreePort = GetFreePort;
        ColorMCGui.PhoneGetFrp = PhoneGetFrp;
    }

    private static int GetFreePort()
    {
        var serverSocket = new ServerSocket(0);
        int port = serverSocket.LocalPort;
        serverSocket.Close();
        return port;
    }

    private static string PhoneGetFrp(FrpType type)
    {
        if (type == FrpType.OpenFrp)
        {
            return NativeLibDir + "/libfrpc_openfrp.so";
        }
        return NativeLibDir + "/libfrpc.so";
    }

    public static void PhoneJvmInstall(Stream stream, string file, ColorMCCore.ZipUpdate? zip)
    {
        new JavaUnpack() { ZipUpdate = zip }.Unpack(stream, file);
    }

    public static string PhoneGetDataDir()
    {
        return AppContext.BaseDirectory;
    }

    public static Process PhoneStartJvm(string file)
    {
        var info = new ProcessStartInfo(NativeLibDir + "/libcolormcnative.so");
        //var info = new ProcessStartInfo(file);
        var path = Path.GetFullPath(new FileInfo(file).Directory.Parent.FullName);

        var path1 = JavaUnpack.GetLibPath(path);

        var temp1 = Os.Getenv("PATH");

        var LD_LIBRARY_PATH = $"{path1}/{(File.Exists($"{path1}/server/libjvm.so") ? "server" : "client")}"
            + $":{path}:{path1}/jli:{path1}:"
            + "/system/lib64:/vendor/lib64:/vendor/lib64/hw:"
            + NativeLibDir;

        info.Environment.Add("LD_LIBRARY_PATH", LD_LIBRARY_PATH);
        info.Environment.Add("PATH", path1 + "/bin:" + temp1);
        info.Environment.Add("JAVA_HOME", path);
        info.Environment.Add("NATIVE_DIR", NativeLibDir);
        info.Environment.Add("HOME", ColorMCCore.BaseDir);
        info.Environment.Add("TMPDIR", CacheDir);
        info.ArgumentList.Add("-Djava.home=" + path);

        var p = new Process
        {
            StartInfo = info,
            EnableRaisingEvents = true
        };
        return p;
    }

    public static Process PhoneJvmRun(GameSettingObj obj, JavaInfo jvm, string dir, List<string> arg, Dictionary<string, string> env)
    {
        var p = PhoneStartJvm(jvm.Path);

        foreach (var item in env)
        {
            p.StartInfo.Environment.Add(item.Key, item.Value);
        }

        p.StartInfo.WorkingDirectory = dir;
        p.StartInfo.ArgumentList.Add("-Djava.io.tmpdir=" + CacheDir);
        p.StartInfo.ArgumentList.Add("-Djna.boot.library.path=" + NativeLibDir);
        p.StartInfo.ArgumentList.Add("-Duser.home=" + ExternalFilesDir);
        p.StartInfo.ArgumentList.Add("-Duser.language=" + Java.Lang.JavaSystem.GetProperty("user.language"));
        p.StartInfo.ArgumentList.Add("-Dos.name=Linux");
        p.StartInfo.ArgumentList.Add("-Dos.version=Android-" + ColorMCCore.Version);
        p.StartInfo.ArgumentList.Add("-Duser.timezone=" + Java.Util.TimeZone.Default.ID);
        arg.ForEach(p.StartInfo.ArgumentList.Add);

        return p;
    }

    /// <summary>
    /// 保持splash不开启
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public static void ConfigSet(GameSettingObj obj)
    {
        //var dir = obj.GetConfigPath();
        //Directory.CreateDirectory(dir);
        //var file = dir + "splash.properties";
        //string data = PathHelper.ReadText(file) ?? "enabled=true";
        //if (data.Contains("enabled=true"))
        //{
        //    PathHelper.WriteText(file, data.Replace("enabled=true", "enabled=false"));
        //}
    }

    public static void ReplaceClassPath(GameSettingObj obj, List<string> list)
    {
        var version = VersionPath.GetVersion(obj.Version)!;
        string dir = obj.GetLogPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var file = obj.GetOptionsFile();
        if (!File.Exists(file))
        {
            File.WriteAllBytes(file, Resource1.options);
        }

        var classpath = false;
        for (int a = 0; a < list.Count; a++)
        {
            list[a] = list[a].Replace("%natives_directory%", NativeLibDir);
            if (list[a].StartsWith("-cp"))
            {
                classpath = true;
                continue;
            }
            if (classpath)
            {
                classpath = false;

                string lwjgl = ResourceUnPack.ComponentsDir + "/lwjgl3/lwjgl-glfw-classes.jar";

                if (PhoneConfigUtils.Config.LwjglVk)
                {
                    lwjgl += ":" + ResourceUnPack.ComponentsDir + "/lwjgl3/lwjgl-vulkan.jar" + ":"
                        + ResourceUnPack.ComponentsDir + "/lwjgl3/lwjgl-vulkan-native.jar";
                }

                list[a] = lwjgl + ":" + list[a];
            }
        }
    }

    public static Process BuildRunProcess(GameRender.RenderType render, DisplayMetrics display, List<string> list, GameSettingObj obj, JavaInfo jvm, Dictionary<string, string> env, bool cacio)
    {
        var version = VersionPath.GetVersion(obj.Version)!;

        var list1 = new List<string>
        {
            "-Dorg.lwjgl.vulkan.libname=libvulkan.so",
            "-Dglfwstub.initEgl=false",
            "-Dlog4j2.formatMsgNoLookups=true",
            "-Dfml.earlyprogresswindow=false",
            "-Dloader.disable_forked_guis=true",
            $"-Dorg.lwjgl.opengl.libname={render.GetFileName()}"
        };

        if (cacio)
        {
            ResourceUnPack.GetCacioJavaArgs(list1, display.WidthPixels, display.HeightPixels,
                jvm.MajorVersion == 8);
        }

        list1.AddRange(list);

        var p = PhoneJvmRun(obj, jvm, obj.GetGamePath(), list1, env);

        p.StartInfo.Environment.Add("glfwstub.windowWidth", $"{display.WidthPixels}");
        p.StartInfo.Environment.Add("glfwstub.windowHeight", $"{display.HeightPixels}");
        p.StartInfo.Environment.Add("ANDROID_VERSION", $"{(int)Build.VERSION.SdkInt}");
        if (CheckHelpers.IsGameVersionV2(version))
        {
            p.StartInfo.Environment.Add("GAME_V2", "1");
        }

        return p;
    }

    private static void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log.Error("Game Pipe", e.Data ?? "null");
    }

    private static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log.Info("Game Pipe", e.Data ?? "null");
    }

    public static IGameHandel PhoneGameLaunch(LoginObj login, GameSettingObj obj, JavaInfo jvm, List<string> list,
   Dictionary<string, string> env)
    {
        if (PhoneConfigUtils.Config.GameRender == GameRenderBG.Pojav)
        {
            var intent = new Intent("ColorMC.Android.Game");
            //加入参数，传递给AnotherActivity
            intent.PutExtra("data","我是传过来的参数");
            Activity.StartActivity(intent);　

            return new PojavHandel(obj);
        }
        AndroidHelper.Main.Post(() =>
        {
            var intent = new Intent(Activity, typeof(GameActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            Activity.StartActivity(intent);
        });

        var render = GameRender.RenderType.gl4es;
        ConfigSet(obj);
        ReplaceClassPath(obj, list);

        while (Game == null)
        {
            Thread.Sleep(100);
        }

        var display = AndroidHelper.GetDisplayMetrics(Game);
        Bitmap bitmap;
        var image = obj.GetIconFile();
        if (File.Exists(image))
        {
            bitmap = BitmapFactory.DecodeFile(image);
        }
        else
        {
            bitmap = BitmapFactory.DecodeResource(Activity.Resources, Resource.Drawable.icon);
        }

        var p = BuildRunProcess(render, display, list, obj, jvm, env, false);
        var game = new GameRender(FilesDir, obj.UUID, obj.Name,
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
            Activity.Update();
        };

        Activity.Update();

        return new PhoneHandel(login, obj, p);
    }

    private static void Game_GameReady(string uuid)
    {
        AndroidHelper.Main.Post(() =>
        {
            Game.StartDisplay(uuid);
        });
    }

    public class PojavHandel(GameSettingObj obj) : IGameHandel
    {
        public string UUID => obj.UUID;

        public bool IsExit => false;

        public nint Handel => IntPtr.Zero;

        public void Kill()
        {
            
        }
    }

    public class PhoneHandel : IGameHandel
    {
        public string UUID { get; init; }

        public bool IsExit => false;

        public nint Handel => IntPtr.Zero;

        public PhoneHandel(LoginObj login, GameSettingObj obj, Process process)
        {
            UUID = obj.UUID;

            var use = true;
            if (use)
            {
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += (a, b) =>
                {
                    ColorMCCore.OnGameLog(obj, b.Data);
                };
                process.ErrorDataReceived += (a, b) =>
                {
                    ColorMCCore.OnGameLog(obj, b.Data);
                };
                process.Exited += (a, b) =>
                {
                    ColorMCCore.OnGameExit(obj, login, process.ExitCode);
                    process.Dispose();
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            else
            {
                var serviceIntent = new Intent(Activity, typeof(GameService));
                serviceIntent.PutExtra("Args", process.StartInfo.ArgumentList.ToArray());
                serviceIntent.PutExtra("Keys", process.StartInfo.Environment.Keys.ToArray());
                serviceIntent.PutExtra("Values", process.StartInfo.Environment.Values.ToArray());

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Activity.StartForegroundService(serviceIntent);
                }
                else
                {
                    Activity.StartService(serviceIntent);
                }
            }
        }

        public void Kill()
        {
            
        }
    }
}
