using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using ColorMC.Android.GameButton;
using ColorMC.Android.UI.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.UI;

public class ButtonEditDialog 
{
    private readonly GameActivity _context;
    private readonly AlertDialog dialog;
    private readonly ButtonData data;
    private readonly EditText editText1;
    private readonly EditText editText2;
    private readonly EditText editText3;
    private readonly EditText editText4;
    private readonly EditText editText5;
    private readonly EditText editText6;

    public ButtonEditDialog(GameActivity context, ButtonData button)
    {
        _context = context;
        data = button;

        var customizeDialog = new AlertDialog.Builder(context);
        var dialogView = LayoutInflater.From(context)!
            .Inflate(Resource.Layout.button_edit, null)!;

        editText1 = dialogView.FindViewById<EditText>(Resource.Id.editText1)!;
        editText2 = dialogView.FindViewById<EditText>(Resource.Id.editText2)!;
        editText3 = dialogView.FindViewById<EditText>(Resource.Id.editText3)!;
        editText4 = dialogView.FindViewById<EditText>(Resource.Id.editText4)!;
        editText5 = dialogView.FindViewById<EditText>(Resource.Id.editText5)!;
        editText6 = dialogView.FindViewById<EditText>(Resource.Id.editText6)!;

        editText5.Text = button.Width.ToString();
        editText6.Text = button.Height.ToString();

        customizeDialog.SetView(dialogView);
        dialog = customizeDialog.Show()!;
    }
}
