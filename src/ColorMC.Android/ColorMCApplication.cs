using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Java.Lang;

namespace ColorMC.Android;

[Application]
public class ColorMCApplication : Application
{
    public ColorMCApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
        // 进行必要的初始化工作
        // 比如加载本地库
        //JavaSystem.LoadLibrary("colormcnative_launch.so");
    }
}