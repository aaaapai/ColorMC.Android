using Android.OS;
using Android.Util;

namespace ColorMC.Android.GLRender;

public static class AndroidHelper
{
    public static readonly Handler Main = new(Looper.MainLooper);

    public static DisplayMetrics GetDisplayMetrics(Activity activity)
    {
        var displayMetrics = new DisplayMetrics();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.N
            && (activity.IsInMultiWindowMode || activity.IsInPictureInPictureMode))
        {
            //For devices with free form/split screen, we need window size, not screen size.
            displayMetrics = activity.Resources.DisplayMetrics;
        }
        else
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                activity.Display.GetRealMetrics(displayMetrics);
            }
            else
            { // Removed the clause for devices with unofficial notch support, since it also ruins all devices with virtual nav bars before P
                activity.WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
            }
        }
        return displayMetrics;
    }
}
