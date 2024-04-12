using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Avalonia.Layout;
using ColorMC.Android.GameButton;
using ColorMC.Android.GLRender;
using ColorMC.Android.UI.Activity;
using ColorMC.Android.UI.Adapter;
using Java.Lang;
using Java.Lang.Reflect;
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
    private readonly ButtonListAdapter adapter;
    private readonly ButtonListAdapter adapter1;
    private readonly EditText editText1;
    private readonly EditText editText2;
    private readonly EditText editText3;
    private readonly EditText editText4;
    private readonly EditText editText5;
    private readonly EditText editText6;
    private readonly RadioButton radioButton1;
    private readonly RadioButton radioButton2;
    private readonly RadioButton radioButton3;
    private readonly RadioButton radioButton4;
    private readonly RadioButton radioButton5;
    private readonly RadioButton radioButton6;
    private readonly RadioButton radioButton7;
    private readonly RadioButton radioButton8;
    private readonly RadioButton radioButton9;
    private readonly Spinner spinner1;
    private readonly Spinner spinner2;

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

        radioButton1 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton1)!;
        radioButton2 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton2)!;
        radioButton3 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton3)!;
        radioButton4 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton4)!;
        radioButton5 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton5)!;
        radioButton6 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton6)!;
        radioButton7 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton7)!;
        radioButton8 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton8)!;
        radioButton9 = dialogView.FindViewById<RadioButton>(Resource.Id.radioButton9)!;

        spinner1 = dialogView.FindViewById<Spinner>(Resource.Id.spinner1)!;
        spinner2 = dialogView.FindViewById<Spinner>(Resource.Id.spinner2)!;

        var groups = new List<string> { "设置", "上一按键布局", "下一按键布局", "循环按钮布局", "键盘输入按钮", "鼠标输入按钮" };

        adapter = new ButtonListAdapter(context, Resource.Layout.button_list_item, groups);

        adapter1 = new ButtonListAdapter(context, Resource.Layout.button_list_item, []);

        spinner1.Adapter = adapter;
        spinner1.SetSelection((int)button.Type, false);
        spinner2.Adapter = adapter1;

        SwitchType(button.Type, button.Data);
        SwitchPos();

        editText1.Text = button.Margin.Left.ToString();
        editText2.Text = button.Margin.Top.ToString();
        editText3.Text = button.Margin.Right.ToString();
        editText4.Text = button.Margin.Bottom.ToString();
        editText5.Text = button.Width.ToString();
        editText6.Text = button.Height.ToString();

        radioButton1.Click += RadioButton1_Click;
        radioButton2.Click += RadioButton1_Click;
        radioButton3.Click += RadioButton1_Click;
        radioButton4.Click += RadioButton1_Click;
        radioButton5.Click += RadioButton1_Click;
        radioButton6.Click += RadioButton1_Click;
        radioButton7.Click += RadioButton1_Click;
        radioButton8.Click += RadioButton1_Click;
        radioButton9.Click += RadioButton1_Click;

        customizeDialog.SetView(dialogView);
        dialog = customizeDialog.Show()!;
    }

    private void RadioButton1_Click(object? sender, EventArgs e)
    {
        if (sender == radioButton1)
        {
            data.Vertical = VerticalAlignment.Top;
            data.Horizontal = HorizontalAlignment.Left;
        }
        else if (sender == radioButton2)
        {
            data.Vertical = VerticalAlignment.Top;
            data.Horizontal = HorizontalAlignment.Center;
        }
        else if (sender == radioButton3)
        {
            data.Vertical = VerticalAlignment.Top;
            data.Horizontal = HorizontalAlignment.Right;
        }
        else if (sender == radioButton4)
        {
            data.Vertical = VerticalAlignment.Center;
            data.Horizontal = HorizontalAlignment.Left;
        }
        else if (sender == radioButton5)
        {
            data.Vertical = VerticalAlignment.Center;
            data.Horizontal = HorizontalAlignment.Center;
        }
        else if (sender == radioButton6)
        {
            data.Vertical = VerticalAlignment.Center;
            data.Horizontal = HorizontalAlignment.Right;
        }
        else if (sender == radioButton7)
        {
            data.Vertical = VerticalAlignment.Bottom;
            data.Horizontal = HorizontalAlignment.Left;
        }
        else if (sender == radioButton8)
        {
            data.Vertical = VerticalAlignment.Bottom;
            data.Horizontal = HorizontalAlignment.Center;
        }
        else if (sender == radioButton9)
        {
            data.Vertical = VerticalAlignment.Bottom;
            data.Horizontal = HorizontalAlignment.Right;
        }

        SwitchPos();
    }

    private void SwitchPos()
    {
        if (data.Vertical == VerticalAlignment.Top)
        {
            radioButton1.Checked = data.Horizontal == HorizontalAlignment.Left;
            radioButton2.Checked = data.Horizontal == HorizontalAlignment.Center;
            radioButton3.Checked = data.Horizontal == HorizontalAlignment.Right;
            radioButton4.Checked = false;
            radioButton5.Checked = false;
            radioButton6.Checked = false;
            radioButton7.Checked = false;
            radioButton8.Checked = false;
            radioButton9.Checked = false;
        }
        else if (data.Vertical == VerticalAlignment.Center)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = data.Horizontal == HorizontalAlignment.Left;
            radioButton5.Checked = data.Horizontal == HorizontalAlignment.Center;
            radioButton6.Checked = data.Horizontal == HorizontalAlignment.Right;
            radioButton7.Checked = false;
            radioButton8.Checked = false;
            radioButton9.Checked = false;
        }
        else if (data.Vertical == VerticalAlignment.Bottom)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            radioButton5.Checked = false;
            radioButton6.Checked = false;
            radioButton7.Checked = data.Horizontal == HorizontalAlignment.Left;
            radioButton8.Checked = data.Horizontal == HorizontalAlignment.Center;
            radioButton9.Checked = data.Horizontal == HorizontalAlignment.Right;
        }
    }

    private void SwitchType(ButtonData.ButtonType type, short code)
    {
        adapter1.Clear();
        if (type == ButtonData.ButtonType.InputKey)
        {
            adapter1.AddAll(LwjglKeycode.KeyNames.Values);
            if (code != 0)
            {
                spinner2.SetSelection(LwjglKeycode.KeyCodes.IndexOf(data.Data), false);
            }
            else
            {
                spinner2.SetSelection(-1, false);
            }
            spinner2.Enabled = true;
        }
        else if (type == ButtonData.ButtonType.MouseKey)
        {
            adapter1.AddAll(LwjglKeycode.MouseNames.Values);
            if (code != -1)
            {
                spinner2.SetSelection(LwjglKeycode.MouseCodes.IndexOf(code), false);
            }
            else
            {
                spinner2.SetSelection(-1, false);
            }
            spinner2.Enabled = true;
        }
        else 
        {
            spinner2.Enabled = false;
        }
    }

    private void SetColor()
    {
        // 尝试使用反射创建ColorPickerDialog.Builder的实例并调用方法
        //try
        //{
        //    // 加载ColorPickerDialog.Builder类
        //    Class builderClass = Class.ForName("com.yourlibrary.ColorPickerDialog$Builder");

        //    // 获取构造函数，假设它有一个Context类型的参数
        //    Constructor constructor = builderClass.GetConstructor(Class.FromType(typeof(Context)));

        //    // 创建实例
        //    Java.Lang.Object builderInstance = constructor.NewInstance(_context);

        //    // 以下为调用方法的示例
        //    // setTitle
        //    Method setTitleMethod = builderClass.GetMethod("setTitle", Class.FromType(typeof(string)));
        //    setTitleMethod.Invoke(builderInstance, "ColorPicker Dialog");

        //    // setPreferenceName
        //    Method setPreferenceNameMethod = builderClass.GetMethod("setPreferenceName", Class.FromType(typeof(string)));
        //    setPreferenceNameMethod.Invoke(builderInstance, "MyColorPickerDialog");

        //    // setPositiveButton（假设该方法无参数）
        //    Method setPositiveButtonMethod = builderClass.GetMethod("setPositiveButton", );
        //    setPositiveButtonMethod.Invoke(builderInstance);

        //    // setNegativeButton（假设该方法无参数）
        //    Method setNegativeButtonMethod = builderClass.GetMethod("setNegativeButton");
        //    setNegativeButtonMethod.Invoke(builderInstance);

        //    // attachAlphaSlideBar
        //    Method attachAlphaSlideBarMethod = builderClass.GetMethod("attachAlphaSlideBar", Class.FromType(typeof(bool)));
        //    attachAlphaSlideBarMethod.Invoke(builderInstance, true);

        //    // attachBrightnessSlideBar
        //    Method attachBrightnessSlideBarMethod = builderClass.GetMethod("attachBrightnessSlideBar", Class.FromType(typeof(bool)));
        //    attachBrightnessSlideBarMethod.Invoke(builderInstance, true);

        //    // setBottomSpace
        //    Method setBottomSpaceMethod = builderClass.GetMethod("setBottomSpace", Class.FromType(typeof(int)));
        //    setBottomSpaceMethod.Invoke(builderInstance, 12);

        //    // show（假设该方法无参数）
        //    Method showMethod = builderClass.GetMethod("show");
        //    showMethod.Invoke(builderInstance);
        //}
        //catch (ClassNotFoundException e)
        //{
        //    // 处理类未找到异常
        //}
        //catch (NoSuchMethodException e)
        //{
        //    // 处理方法未找到异常
        //}
        //catch (InvocationTargetException e)
        //{
        //    // 处理调用目标异常
        //}
        //catch (IllegalAccessException e)
        //{
        //    // 处理非法访问异常
        //}
        //catch (InstantiationException e)
        //{
        //    // 处理实例化异常
        //}
    }
}
