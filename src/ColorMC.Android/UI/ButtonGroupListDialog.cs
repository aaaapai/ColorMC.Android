using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using ColorMC.Android.GameButton;
using ColorMC.Android.UI.Activity;
using ColorMC.Android.UI.Adapter;

namespace ColorMC.Android.UI;

public class ButtonGroupListDialog : Java.Lang.Object, AdapterView.IOnItemClickListener, AdapterView.IOnItemLongClickListener
{
    private readonly GameActivity _context;
    private readonly AlertDialog dialog;
    private readonly ButtonListAdapter adapter;

    public ButtonGroupListDialog(GameActivity context)
    {
        _context = context;

        var customizeDialog = new AlertDialog.Builder(context);
        var dialogView = LayoutInflater.From(context)!
            .Inflate(Resource.Layout.button_group_list, null)!;
        var button1 = dialogView.FindViewById<Button>(Resource.Id.button1)!;
        var button2 = dialogView.FindViewById<Button>(Resource.Id.button2)!;
        var listview1 = dialogView.FindViewById<ListView>(Resource.Id.listView1)!;

        button1.Click += Button1_Click;
        button2.Click += Button2_Click;

        var groups = ButtonManage.ButtonGroups.Keys.ToList();

        adapter = new ButtonListAdapter(context, Resource.Layout.button_list_item, groups);
        listview1.Adapter = adapter;
        listview1.OnItemClickListener = this;

        customizeDialog.SetView(dialogView);
        dialog = customizeDialog.Show()!;
    }

    private void Button1_Click(object? sender, EventArgs e)
    {
        var editText1 = new EditText(_context);
        AlertDialog? dialog = null;
        var inputDialog = new AlertDialog.Builder(_context)!;
        inputDialog.SetTitle(Resource.String.setting_info1)!
            .SetView(editText1)!
            .SetPositiveButton("确定", new ClickListener(() =>
            {
                var data = editText1.Text?.ToString();
                dialog!.Dismiss();
                if (string.IsNullOrWhiteSpace(data))
                {
                    DialogHelper.ShowInfo(_context, "新建按钮组", "输入的内容为空");
                    return;
                }
                if (ButtonManage.ButtonLayouts.ContainsKey(data))
                {
                    DialogHelper.ShowInfo(_context, "新建按钮组", "按键布局已存在");
                    return;
                }

                ButtonManage.NewGroup(data);
                adapter.Add(data);
                DialogHelper.Notify(_context, "已添加按键组");
            }))!
            .SetNegativeButton("取消", new ClickListener(() =>
            {
                dialog!.Dismiss();
            }));
        dialog = inputDialog.Show();
    }

    private void Button2_Click(object? sender, EventArgs e)
    {
        dialog.Dismiss();
        _context.ShowLayoutList();
    }

    void AdapterView.IOnItemClickListener.OnItemClick(AdapterView? parent, View? view, int position, long id)
    {
        var result = (view as TextView)?.Text?.ToString();
        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        dialog.Dismiss();
        _context.EditGroup(result);
    }

    public bool OnItemLongClick(AdapterView? parent, View? view, int position, long id)
    {
        var result = (view as TextView)?.Text?.ToString();
        if (string.IsNullOrWhiteSpace(result))
        {
            return true;
        }

        DialogHelper.YesNo(_context, "删除按键组", $"是否要删除按键组 {result}", (res) =>
        {
            if (res)
            {
                ButtonManage.RemoveGroup(result);
                adapter.Remove(result);
            }
        });

        return true;
    }
}
