using ColorMC.Core.Objs;
using ColorMC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Android.Systems;
using ColorMC.Android.UI.Activity;
using Android.Content.PM;
using ColorMC.Gui.Objs;
using ColorMC.Gui;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Avalonia.Threading;
using ColorMC.Android.components;
using ColorMC.Android.GLRender;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using Process = System.Diagnostics.Process;
using Path = System.IO.Path;
using Java.Util;
using Android.Util;
using static ColorMC.Core.Objs.Minecraft.GameArgObj.Arguments;

namespace ColorMC.Android;

public static class ColorMCAndroid
{
    public static void Init()
    {
        ColorMCCore.PhoneJvmInstall = PhoneJvmInstall;
        ColorMCCore.PhoneStartJvm = PhoneStartJvm;
        ColorMCCore.PhoneGetDataDir = PhoneGetDataDir;
        ColorMCCore.PhoneJvmRun = PhoneJvmRun;

        ColorMCGui.PhoneGetFrp = PhoneGetFrp;
    }

    private static string PhoneGetFrp(FrpType type)
    {
        if (type == FrpType.OpenFrp)
        {
            return MainActivity.NativeLibDir + "/libfrpc_openfrp.so";
        }
        return MainActivity.NativeLibDir + "/libfrpc.so";
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
        var info = new ProcessStartInfo(MainActivity.NativeLibDir + "/libcolormcnative.so");
        var path = Path.GetFullPath(new FileInfo(file).Directory.Parent.FullName);

        var path1 = JavaUnpack.GetLibPath(path);

        var temp1 = Os.Getenv("PATH");

        var LD_LIBRARY_PATH = $"{path1}/{(File.Exists($"{path1}/server/libjvm.so") ? "server" : "client")}"
            + $":{path}:{path1}/jli:{path1}:"
            + "/system/lib64:/vendor/lib64:/vendor/lib64/hw:"
            + MainActivity.NativeLibDir;

        info.Environment.Add("LD_LIBRARY_PATH", LD_LIBRARY_PATH);
        info.Environment.Add("PATH", path1 + "/bin:" + temp1);
        info.Environment.Add("JAVA_HOME", path);
        info.Environment.Add("NATIVE_DIR", MainActivity.NativeLibDir);
        info.Environment.Add("HOME", ColorMCCore.BaseDir);
        info.Environment.Add("TMPDIR", MainActivity.CacheDir);
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
        p.StartInfo.ArgumentList.Add("-Djava.io.tmpdir=" + MainActivity.CacheDir);
        p.StartInfo.ArgumentList.Add("-Djna.boot.library.path=" + MainActivity.NativeLibDir);
        p.StartInfo.ArgumentList.Add("-Duser.home=" + MainActivity.ExternalFilesDir);
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

        var native = MainActivity.NativeLibDir;
        var classpath = false;
        for (int a = 0; a < list.Count; a++)
        {
            list[a] = list[a].Replace("%natives_directory%", native);
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
}
