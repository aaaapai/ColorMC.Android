using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Avalonia.Layout;
using System;

namespace ColorMC.Android.GameButton;

public class ButtonView : View, View.IOnTouchListener
{
    private readonly ButtonData _data;
    private Paint _paint;
    private Bitmap? _bitmap;
    private Matrix? _matrix;

    private readonly int RenderWidth, RenderHeight;
    private readonly GestureDetector gestureDetector;

    private class GestureListener(ButtonData data, IButtonFuntion func) : GestureDetector.SimpleOnGestureListener
    {
        public override bool OnDoubleTap(MotionEvent e)
        {
            if (func.IsEdit)
            {
                func.GoEdit(data);
            }

            return base.OnDoubleTap(e);
        }

        public override bool OnSingleTapConfirmed(MotionEvent e)
        {
            if (!func.IsEdit)
            {
                switch (data.Type)
                {
                    case ButtonData.ButtonType.Setting:
                        func.ShowSetting();
                        break;
                    case ButtonData.ButtonType.LastGroup:
                        func.LastGroup();
                        break;
                    case ButtonData.ButtonType.NextGroup:
                        func.NextGroup();
                        break;
                }
            }

            return base.OnSingleTapConfirmed(e);
        }
    }

    public ButtonView(ButtonData data, IButtonFuntion func, Context? context) : base(context)
    {
        _data = data;

        RenderWidth = DpToPx(_data.Width);
        RenderHeight = DpToPx(_data.Height);

        _paint = new Paint(PaintFlags.AntiAlias)
        {
            Color = Color.ParseColor(_data.Foreground ?? "#FFFFFF"),
            TextSize = DpToPx(_data.TextSize),
            TextAlign = Paint.Align.Center
        };

        SetBackgroundColor(Color.ParseColor(_data.Backgroud ?? "#000000"));

        if (!string.IsNullOrWhiteSpace(_data.Image))
        {
            try
            {
                var bytes = Convert.FromBase64String(_data.Image);
                _bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            }
            catch
            {

            }
        }
        else if (_data.Type == ButtonData.ButtonType.Setting)
        {
            _bitmap = BitmapFactory.DecodeResource(Resources, _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.icon);
        }

        if (_bitmap != null)
        {
            int down = DpToPx(1);
            // 计算缩放比例和图片位置
            int viewWidth = RenderWidth - down * 2; // 减去两侧的间距
            int viewHeight = RenderHeight - down * 2; // 减去上下的间距
            _matrix = new();
            if (viewHeight > 0 && viewHeight > 0)
            {
                float scaleWidth = viewWidth / (float)_bitmap.Width;
                float scaleHeight = viewHeight / (float)_bitmap.Height;
                float scale = Math.Min(scaleWidth, scaleHeight); // 保持图片比例不变


                _matrix.SetScale(scale, scale);

                // 计算图片居中的偏移量
                float dx = (viewWidth - _bitmap.Width * scale) / 2;
                float dy = (viewHeight - _bitmap.Height * scale) / 2;
                _matrix.PostTranslate(dx + down, dy + down); // 添加边框间距
            }
            else
            {
                _matrix.Reset();
            }
        }

        gestureDetector = new GestureDetector(context, new GestureListener(data, func));
        SetOnTouchListener(this);
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();

        var layoutParams = new RelativeLayout.LayoutParams(RenderWidth, RenderHeight);

        if (_data.Horizontal == HorizontalAlignment.Center
            && _data.Vertical == VerticalAlignment.Center)
        {
            layoutParams.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);
        }
        else
        {
            switch (_data.Horizontal)
            {
                case HorizontalAlignment.Left:
                    layoutParams.AddRule(LayoutRules.AlignParentStart, (int)LayoutRules.True);
                    break;
                case HorizontalAlignment.Center:
                    layoutParams.AddRule(LayoutRules.CenterHorizontal, (int)LayoutRules.True);
                    break;
                case HorizontalAlignment.Right:
                    layoutParams.AddRule(LayoutRules.AlignParentEnd, (int)LayoutRules.True);
                    break;
            }
            switch (_data.Vertical)
            {
                case VerticalAlignment.Top:
                    layoutParams.AddRule(LayoutRules.AlignParentTop, (int)LayoutRules.True);
                    break;
                case VerticalAlignment.Center:
                    layoutParams.AddRule(LayoutRules.CenterVertical, (int)LayoutRules.True);
                    break;
                case VerticalAlignment.Bottom:
                    layoutParams.AddRule(LayoutRules.AlignParentBottom, (int)LayoutRules.True);
                    break;
            }
        }

        layoutParams.SetMargins(DpToPx(_data.Margin.Left), DpToPx(_data.Margin.Top),
            DpToPx(_data.Margin.Right), DpToPx(_data.Margin.Bottom));

        LayoutParameters = layoutParams;
        Alpha = _data.Alpha;
    }

    private int DpToPx(float dp)
    {
        float density = Context?.Resources?.DisplayMetrics?.Density ?? 1f;
        return (int)Math.Round(dp * density);
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);

        if (_bitmap != null && _matrix != null)
        {
            canvas.DrawBitmap(_bitmap, _matrix, null);
        }

        if (_data.Content != null)
        {
            float baseLineY = RenderHeight / 2 - (_paint.Descent() + _paint.Ascent()) / 2;
            canvas.DrawText(_data.Content, RenderWidth / 2, baseLineY, _paint);
        }
    }

    public bool OnTouch(View? v, MotionEvent? e)
    {
        return gestureDetector.OnTouchEvent(e!);
    }
}
