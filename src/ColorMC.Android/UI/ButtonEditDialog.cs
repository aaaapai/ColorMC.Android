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
    public ButtonEditDialog(GameActivity context, ButtonData button)
    {
        _context = context;
        data = button;

        var customizeDialog = new AlertDialog.Builder(context);
        var dialogView = LayoutInflater.From(context)!
            .Inflate(Resource.Layout.button_edit, null)!;
    }
}
