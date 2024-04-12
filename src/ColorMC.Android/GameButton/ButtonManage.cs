using Avalonia.Layout;
using ColorMC.Android.UI.Activity;
using Newtonsoft.Json;
using ColorMC.Android.GLRender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.GameButton;

public static class ButtonManage
{
    public static Dictionary<string, ButtonGroup> ButtonGroups = [];
    public static Dictionary<string, ButtonLayout> ButtonLayouts = [];

    public static string GroupPath;
    public static string LayoutPath;

    public static void Load()
    {
        GroupPath = MainActivity.ExternalFilesDir + "/buttons/groups";
        LayoutPath = MainActivity.ExternalFilesDir + "/buttons/layouts";

        Directory.CreateDirectory(GroupPath);
        Directory.CreateDirectory(LayoutPath);

        var list = Directory.GetFiles(GroupPath);
        if (list.Length == 0)
        {
            var group = GenDefaultGroup();
            ButtonGroups.Add(group.Name, group);
            SaveGroup(group);
        }
        else
        {
            foreach (var item in list)
            {
                if (item.EndsWith(".json"))
                {
                    try
                    {
                        var data = File.ReadAllText(item);
                        var group = JsonConvert.DeserializeObject<ButtonGroup>(data);
                        if (group == null || string.IsNullOrWhiteSpace(group.Name))
                        {
                            continue;
                        }
                        group.Layouts ??= new();
                        if (!ButtonGroups.TryAdd(group.Name, group))
                        {
                            ButtonGroups[group.Name] = group;
                        }
                    }
                    catch
                    { 
                    
                    }
                }
            }
        }

        list = Directory.GetFiles(LayoutPath);
        if (list.Length == 0)
        {
            var layout = GenDefaultLayout();
            ButtonLayouts.Add(layout.Name, layout);
            SaveLayout(layout);
            layout = GenDefaultMouseLayout();
            ButtonLayouts.Add(layout.Name, layout);
            SaveLayout(layout);
        }
        else
        {
            foreach (var item in list)
            {
                if (item.EndsWith(".json"))
                {
                    try
                    {
                        var data = File.ReadAllText(item);
                        var layout = JsonConvert.DeserializeObject<ButtonLayout>(data);
                        if (layout == null || string.IsNullOrWhiteSpace(layout.Name))
                        {
                            continue;
                        }
                        if (!ButtonLayouts.TryAdd(layout.Name, layout))
                        {
                            ButtonLayouts[layout.Name] = layout;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
    }

    public static void SaveGroup(ButtonGroup group)
    {
        File.WriteAllText(GroupPath + "/" + group.Name.ToLower() + ".json", JsonConvert.SerializeObject(group));
    }

    public static void SaveLayout(ButtonLayout layout)
    {
        File.WriteAllText(LayoutPath + "/" + layout.Name.ToLower() + ".json", JsonConvert.SerializeObject(layout));
    }

    public static void RemoveLayout(string name)
    {
        if (ButtonLayouts.Remove(name))
        {
            foreach (var item in ButtonGroups.Values)
            {
                bool save = false;
                if (item.MainLayout == name)
                {
                    item.MainLayout = "";
                    save = true;
                }
                if (item.MouseLayout == name)
                {
                    item.MouseLayout = "";
                    save = true;
                }
                if (item.Layouts.RemoveAll((item)=>item == name) > 0)
                {
                    save = true;
                }
                if (save)
                {
                    Task.Run(() =>
                    {
                        SaveGroup(item);
                    });
                }
            }

            string path = LayoutPath + "/" + name.ToLower() + ".json";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public static void RemoveGroup(string name)
    {
        if (ButtonGroups.Remove(name))
        {
            string path = GroupPath + "/" + name.ToLower() + ".json";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public static void NewLayout(string name)
    {
        var layout = new ButtonLayout()
        { 
            Name = name
        };

        if (!ButtonLayouts.TryAdd(name, layout))
        {
            ButtonLayouts[name] = layout;
        }
        SaveLayout(layout);
    }

    public static void NewGroup(string name)
    {
        var group = new ButtonGroup()
        {
            Name = name
        };

        if (!ButtonGroups.TryAdd(name, group))
        {
            ButtonGroups[name] = group;
        }
        SaveGroup(group);
    }

    public static ButtonData GenButton()
    {
        return new ButtonData()
        {
            Type = ButtonData.ButtonType.Key,
            Width = 50,
            Height = 50,
            Horizontal = HorizontalAlignment.Left,
            Vertical = VerticalAlignment.Top,
            Margin = new(5),
            Backgroud = "#343434",
            Shine = true,
            ShineColor = "#EFEFEF",
            Alpha = 1
        };
    }

    private static ButtonLayout GenDefaultLayout()
    {
        return new()
        {
            Name = "DefaultLayout",
            Buttons =
            [
                new ButtonData()
                {
                    Type = ButtonData.ButtonType.Setting,
                    Width = 50,
                    Height = 50,
                    Horizontal = HorizontalAlignment.Right,
                    Vertical = VerticalAlignment.Top,
                    Margin = new(5),
                    Backgroud = "#343434",
                    Alpha = 1
                },
                new()
                {
                    Type = ButtonData.ButtonType.LastGroup,
                    Width = 50,
                    Height = 50,
                    Horizontal = HorizontalAlignment.Right,
                    Vertical = VerticalAlignment.Bottom,
                    Margin = new(5, 5, 60, 5),
                    TextSize = 40,
                    Content = "«",
                    Backgroud = "#343434",
                    Foreground = "#FFFFFF",
                    Shine = true,
                    ShineColor = "#EFEFEF",
                    Alpha = 0.5f
                },
                new()
                {
                    Type = ButtonData.ButtonType.NextGroup,
                    Width = 50,
                    Height = 50,
                    Horizontal = HorizontalAlignment.Right,
                    Vertical = VerticalAlignment.Bottom,
                    Margin = new(5),
                    TextSize = 40,
                    Content = "»",
                    Backgroud = "#343434",
                    Foreground = "#FFFFFF",
                    Shine = true,
                    ShineColor = "#EFEFEF",
                    Alpha = 0.5f
                }
            ]
        };
    }

    private static ButtonLayout GenDefaultMouseLayout()
    {
        return new()
        {
            Name = "DefaultMouseLayout",
            Buttons =
            [
                new ButtonData()
                {
                    Type = ButtonData.ButtonType.Mouse,
                    Data = LwjglKeycode.GLFW_MOUSE_BUTTON_LEFT,
                    Width = 50,
                    Height = 50,
                    Margin = new(5, 5, 5, 5),
                    Horizontal = HorizontalAlignment.Right,
                    Vertical = VerticalAlignment.Top,
                    Content = "左键",
                    Backgroud = "#343434",
                    Foreground = "#FFFFFF",
                    Shine = true,
                    ShineColor = "#EFEFEF",
                    Alpha = 1
                },
                new()
                {
                    Type = ButtonData.ButtonType.Mouse,
                    Width = 50,
                    Height = 50,
                    Horizontal = HorizontalAlignment.Right,
                    Vertical = VerticalAlignment.Top,
                    Margin = new(5, 5, 60, 5),
                    TextSize = 40,
                    Content = "右键",
                    Backgroud = "#343434",
                    Foreground = "#FFFFFF",
                    Shine = true,
                    ShineColor = "#EFEFEF",
                    Alpha = 1f
                },
                new()
                {
                    Type = ButtonData.ButtonType.Key,
                    Data = LwjglKeycode.GLFW_KEY_LEFT_ALT,
                    Width = 50,
                    Height = 50,
                    Horizontal = HorizontalAlignment.Left,
                    Vertical = VerticalAlignment.Top,
                    Margin = new(5),
                    TextSize = 40,
                    Content = "Alt",
                    Backgroud = "#343434",
                    Foreground = "#FFFFFF",
                    Shine = true,
                    ShineColor = "#EFEFEF",
                    Alpha = 1f
                },
                new()
                {
                    Type = ButtonData.ButtonType.Key,
                    Data = LwjglKeycode.GLFW_KEY_LEFT_CONTROL,
                    Width = 50,
                    Height = 50,
                    Horizontal = HorizontalAlignment.Left,
                    Vertical = VerticalAlignment.Top,
                    Margin = new(60, 5, 5, 5),
                    TextSize = 40,
                    Content = "Ctrl",
                    Backgroud = "#343434",
                    Foreground = "#FFFFFF",
                    Shine = true,
                    ShineColor = "#EFEFEF",
                    Alpha = 1f
                },
                new()
                {
                    Type = ButtonData.ButtonType.Key,
                    Data = LwjglKeycode.GLFW_KEY_LEFT_SHIFT,
                    Width = 100,
                    Height = 50,
                    Horizontal = HorizontalAlignment.Left,
                    Vertical = VerticalAlignment.Top,
                    Margin = new(5, 60, 5, 5),
                    TextSize = 40,
                    Content = "Shift",
                    Backgroud = "#343434",
                    Foreground = "#FFFFFF",
                    Shine = true,
                    ShineColor = "#EFEFEF",
                    Alpha = 1f
                }
            ]
        };
    }

    public static ButtonGroup GenDefaultGroup()
    {
        return new()
        {
            Name = "DefaultGroup",
            Layouts =
            [
                "DefaultLayout",
                "DefaultMouseLayout"
            ],
            MainLayout = "DefaultLayout",
            MouseLayout = "DefaultMouseLayout"
        };
    }
}
