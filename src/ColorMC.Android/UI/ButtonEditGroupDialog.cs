using System;
using System.Linq;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using ColorMC.Android.GameButton;
using ColorMC.Android.UI.Activity;
using ColorMC.Android.UI.Adapter;

namespace ColorMC.Android.UI;

public class ButtonEditGroupDialog : Java.Lang.Object, AdapterView.IOnItemLongClickListener
{
    private readonly GameActivity _context;
    private readonly AlertDialog dialog;
    private readonly ButtonListAdapter adapter;
    private readonly ButtonListAdapter adapter1;
    private readonly ButtonListAdapter adapter2;
    private readonly ButtonGroup group;

    public ButtonEditGroupDialog(GameActivity context, string name)
    {
        _context = context;

        var customizeDialog = new AlertDialog.Builder(context);
        var dialogView = LayoutInflater.From(context)!
            .Inflate(Resource.Layout.button_group_list, null)!;
        var listview1 = dialogView.FindViewById<ListView>(Resource.Id.listView1)!;

        var textview1 = dialogView.FindViewById<TextView>(Resource.Id.text_view_1)!;
        textview1.Text = $"修改按键组 {name}";

        var spinner1 = dialogView.FindViewById<Spinner>(Resource.Id.spinner1)!;
        var spinner2 = dialogView.FindViewById<Spinner>(Resource.Id.spinner2)!;

        var button1 = dialogView.FindViewById<Button>(Resource.Id.button1)!;

        group = ButtonManage.ButtonGroups[name];

        var groups = ButtonManage.ButtonLayouts.Keys.ToList();

        adapter = new ButtonListAdapter(context, Resource.Layout.button_list_item, groups);
        adapter1 = new ButtonListAdapter(context, Resource.Layout.button_list_item, groups);

        spinner1.Adapter = adapter;
        spinner2.Adapter = adapter1;

        spinner1.SetSelection(groups.IndexOf(group.MainLayout));
        spinner2.SetSelection(groups.IndexOf(group.MouseLayout));

        spinner1.ItemSelected += Spinner1_ItemSelected;
        spinner2.ItemSelected += Spinner2_ItemSelected;

        button1.Click += Button1_Click;

        listview1.Adapter = adapter2;
        listview1.OnItemLongClickListener = this;

        customizeDialog.SetView(dialogView);
        dialog = customizeDialog.Show()!;
    }

    private void Button1_Click(object? sender, EventArgs e)
    {
        var editText1 = new Spinner(_context);
        var groups = ButtonManage.ButtonLayouts.Keys.ToList();
        groups.Remove(group.MainLayout);
        groups.Remove(group.MouseLayout);
        foreach (var item in group.Layouts)
        {
            groups.Remove(item);
        }
        var adapter = new ButtonListAdapter(_context, Resource.Layout.button_list_item, groups);
        AlertDialog? dialog = null;
        var inputDialog = new AlertDialog.Builder(_context)!;
        inputDialog.SetTitle(Resource.String.setting_info1)!
            .SetView(editText1)!
            .SetPositiveButton("确定", new ClickListener(() =>
            {
                if (editText1.SelectedItemPosition < 0)
                {
                    return;
                }
                var data = groups[editText1.SelectedItemPosition];
                if (string.IsNullOrWhiteSpace(data))
                {
                    return;
                }
                if (group.Layouts.Contains(data))
                {
                    DialogHelper.ShowInfo(_context, "添加子布局", "按键布局已存在");
                    return;
                }

                group.Layouts.Add(data);
                adapter.Add(data);
                DialogHelper.Notify(_context, "已添加子布局");

                dialog!.Dismiss();
            }))!
            .SetNegativeButton("取消", new ClickListener(() =>
            {
                dialog!.Dismiss();
            }));
        dialog = inputDialog.Show();
    }

    private void Spinner2_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        group.MouseLayout = adapter.GetItem(e.Position);
        ButtonManage.SaveGroup(group);
    }

    private void Spinner1_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        group.MainLayout = adapter.GetItem(e.Position);
        ButtonManage.SaveGroup(group);
    }

    public bool OnItemLongClick(AdapterView? parent, View? view, int position, long id)
    {
        var result = (view as TextView)?.Text?.ToString();
        if (string.IsNullOrWhiteSpace(result))
        {
            return true;
        }

        DialogHelper.YesNo(_context, "删除按键布局", $"是否要删除按键布局 {result}", (res) =>
        {
            if (res)
            {
                group.Layouts.Remove(result);
                adapter.Remove(result);
            }
        });

        return true;
    }
}
