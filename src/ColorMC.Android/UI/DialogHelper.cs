using Android.Content;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.UI;

public static class DialogHelper
{
    public static void ShowInfo(Context context, string title, string text)
    {
        var normalDialog = new AlertDialog.Builder(context);
        AlertDialog? dialog = null;
        normalDialog.SetTitle(title);
        normalDialog.SetMessage(text);
        normalDialog.SetPositiveButton("确定", new ClickListener(() =>
        {
            dialog?.Dismiss();
        }));
        dialog = normalDialog.Show();
    }

    public static void YesNo(Context context, string title, string text, Action<bool> res)
    {
        var normalDialog = new AlertDialog.Builder(context);
        AlertDialog? dialog = null;
        normalDialog.SetTitle(title);
        normalDialog.SetMessage(text);
        normalDialog.SetPositiveButton("确定", new ClickListener(() =>
        {
            res(true);
            dialog?.Dismiss();
        }));
        normalDialog.SetPositiveButton("取消", new ClickListener(() =>
        {
            res(false);
            dialog?.Dismiss();
        }));
        dialog = normalDialog.Show();
    }

    public static void Notify(Context context, string text)
    {
        Toast.MakeText(context, text, ToastLength.Short)!.Show();
    }
}

public class ClickListener(Action action) : Java.Lang.Object, IDialogInterfaceOnClickListener
{
    public void OnClick(IDialogInterface? dialog, int which)
    {
        action();
    }
}