using System.Collections.Generic;

namespace ColorMC.Android.GameButton;

public record ButtonGroup
{
    /// <summary>
    /// 布局名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 切换的分组
    /// </summary>
    public List<string> Layouts { get; set; }
    /// <summary>
    /// 主要分组
    /// </summary>
    public string MainLayout { get; set; }
    /// <summary>
    /// 鼠标分组
    /// </summary>
    public string MouseLayout { get; set; }
}
