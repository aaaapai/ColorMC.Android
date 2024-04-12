using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Android.UI.Activity;

namespace ColorMC.Android;

public partial class PhoneControl : UserControl
{
    private readonly MainActivity _activity;
    public PhoneControl(MainActivity activity)
    {
        _activity = activity;

        WrapPanel panel = new();
        Button button = new()
        {
            Width = 100,
            Height = 25,
            Content = "��Ⱦ����",
            Margin = new(0, 0, 5, 0)
        };
        button.Click += Button_Click;
        panel.Children.Add(button);

        button = new()
        {
            Width = 100,
            Height = 25,
            Content = "��������",
            Margin = new(0, 0, 5, 0)
        };
        button.Click += Button1_Click;
        panel.Children.Add(button);

        ToggleSwitch check = new()
        {
            OffContent = "����lwjgl-vulkan",
            OnContent = "����lwjgl-vulkan",
            IsChecked = PhoneConfigUtils.Config.LwjglVk
        };
        check.IsCheckedChanged += Check_IsCheckedChanged;
        panel.Children.Add(check);

        Content = panel;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        _activity.GameSetting();
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        _activity.Setting();
    }

    private void Check_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox check)
        {
            return;
        }

        PhoneConfigUtils.Config.LwjglVk = check.IsChecked == true;
        PhoneConfigUtils.Save();
    }
}
