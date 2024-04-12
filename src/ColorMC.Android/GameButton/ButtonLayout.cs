using Avalonia.Layout;
using System.Collections.Generic;

namespace ColorMC.Android.GameButton;

public record ButtonLayout
{
    /// <summary>
    /// 布局名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 按钮
    /// </summary>
    public List<ButtonData> Buttons { get; set; }
}
