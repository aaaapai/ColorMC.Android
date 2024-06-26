using Avalonia.Layout;

namespace ColorMC.Android.GameButton;

public record MarginData
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public MarginData() { }

    public MarginData(int value)
    {
        Left = value;
        Top = value;
        Right = value;
        Bottom = value;
    }

    public MarginData(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}

public record ButtonData
{
    public enum ButtonType
    {
        /// <summary>
        /// 打开设置
        /// </summary>
        Setting,
        /// <summary>
        /// 切换到上一按键布局
        /// </summary>
        LastGroup,
        /// <summary>
        /// 切换到下一按键布局
        /// </summary>
        NextGroup,
        /// <summary>
        /// 循环按钮布局
        /// </summary>
        LoopGroup,
        /// <summary>
        /// 键盘输入按钮
        /// </summary>
        InputKey,
        /// <summary>
        /// 鼠标输入按钮
        /// </summary>
        MouseKey,
    }

    /// <summary>
    /// 按钮类型
    /// </summary>
    public ButtonType Type { get; set; }
    /// <summary>
    /// 按钮代码
    /// </summary>
    public short Data { get; set; }
    /// <summary>
    /// 显示内容
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 长
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// 高
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// 水平对齐
    /// </summary>
    public HorizontalAlignment Horizontal { get; set; }
    /// <summary>
    /// 垂直对齐
    /// </summary>
    public VerticalAlignment Vertical { get; set; }
    /// <summary>
    /// 边距
    /// </summary>
    public MarginData Margin { get; set; }
    /// <summary>
    /// 背景色
    /// </summary>
    public string Backgroud { get; set; }
    /// <summary>
    /// 前景色
    /// </summary>
    public string Foreground { get; set; }
    /// <summary>
    /// 按下发光
    /// </summary>
    public bool Shine { get; set; }
    /// <summary>
    /// 发光颜色
    /// </summary>
    public string ShineColor { get; set; }
    /// <summary>
    /// 图片
    /// </summary>
    public string Image { get; set; }
    /// <summary>
    /// 文字大小
    /// </summary>
    public float TextSize { get; set; }
    /// <summary>
    /// 透明度
    /// </summary>
    public float Alpha { get; set; }
    /// <summary>
    /// 长按模式
    /// </summary>
    public bool LongClick { get; set; }
    /// <summary>
    /// 长按时间
    /// </summary>
    public int ClickTime { get; set; }
    /// <summary>
    /// 反转输入
    /// </summary>
    public bool Inversion { get; set; }
    /// <summary>
    /// 锁定输入
    /// </summary>
    public bool Lock { get; set; }
    /// <summary>
    /// 循环输入
    /// </summary>
    public bool Loop { get; set; }
    /// <summary>
    /// 循环次数
    /// </summary>
    public int LoopCount { get; set; }
}
