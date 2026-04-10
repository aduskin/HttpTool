using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HttpTool.Desktop.Controls;

/// <summary>
/// 趋势图自定义控件
/// </summary>
public class TrendChart : Control
{
    private const int MaxVisiblePoints = 500;

    static TrendChart()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TrendChart),
            new FrameworkPropertyMetadata(typeof(TrendChart)));
    }

    /// <summary>
    /// 圆角
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(TrendChart),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// 图表区域背景色（独立于控件背景）
    /// </summary>
    public static readonly DependencyProperty ChartBackgroundProperty =
        DependencyProperty.Register(nameof(ChartBackground), typeof(Brush), typeof(TrendChart),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x1a, 0x1a, 0x2e)), FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush ChartBackground
    {
        get => (Brush)GetValue(ChartBackgroundProperty);
        set => SetValue(ChartBackgroundProperty, value);
    }

    /// <summary>
    /// 图表区背景色（别名，兼容旧名称）
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public Brush PlotBackground
    {
        get => ChartBackground;
        set => ChartBackground = value;
    }

    /// <summary>
    /// 线条颜色
    /// </summary>
    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(TrendChart),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x00, 0x7b, 0xff)), FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush LineBrush
    {
        get => (Brush)GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    /// <summary>
    /// 线条粗细
    /// </summary>
    public static readonly DependencyProperty LineThicknessProperty =
        DependencyProperty.Register(nameof(LineThickness), typeof(double), typeof(TrendChart),
            new FrameworkPropertyMetadata(1.5, FrameworkPropertyMetadataOptions.AffectsRender));

    public double LineThickness
    {
        get => (double)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    /// <summary>
    /// 图表标题
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(TrendChart),
            new FrameworkPropertyMetadata("Response Time Trend (ms)", FrameworkPropertyMetadataOptions.AffectsRender));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// 进度值 (0-100)
    /// </summary>
    public static readonly DependencyProperty ProgressValueProperty =
        DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(TrendChart),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    /// <summary>
    /// X 轴最大值
    /// </summary>
    public static readonly DependencyProperty MaxXProperty =
        DependencyProperty.Register(nameof(MaxX), typeof(double), typeof(TrendChart),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double MaxX
    {
        get => (double)GetValue(MaxXProperty);
        set => SetValue(MaxXProperty, value);
    }

    /// <summary>
    /// Y 轴最大值
    /// </summary>
    public static readonly DependencyProperty MaxYProperty =
        DependencyProperty.Register(nameof(MaxY), typeof(double), typeof(TrendChart),
            new FrameworkPropertyMetadata(1000.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double MaxY
    {
        get => (double)GetValue(MaxYProperty);
        set => SetValue(MaxYProperty, value);
    }

    /// <summary>
    /// 数据点
    /// </summary>
    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// 图表数据点
    /// </summary>
    public static readonly DependencyProperty DataPointsProperty =
        DependencyProperty.Register(nameof(DataPoints), typeof(IEnumerable<PointData>), typeof(TrendChart),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public IEnumerable<PointData>? DataPoints
    {
        get => (IEnumerable<PointData>?)GetValue(DataPointsProperty);
        set => SetValue(DataPointsProperty, value);
    }

    public TrendChart()
    {
        MinWidth = 200;
        MinHeight = 100;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var width = ActualWidth;
        var height = ActualHeight;

        if (width <= 0 || height <= 0)
            return;

        // 使用基类的 Margin, Background, BorderBrush, BorderThickness
        var margin = Margin;
        var bg = Background ?? new SolidColorBrush(Colors.Transparent);
        var borderBrush = BorderBrush ?? Brushes.Transparent;
        var borderThickness = BorderThickness;
        var hasBorder = borderThickness.Left > 0 || borderThickness.Top > 0 || borderThickness.Right > 0 || borderThickness.Bottom > 0;

        var contentLeft = margin.Left;
        var contentTop = margin.Top;
        var contentWidth = width - margin.Left - margin.Right;
        var contentHeight = height - margin.Top - margin.Bottom;

        if (contentWidth <= 0 || contentHeight <= 0)
            return;

        var chartPadding = 45.0;
        var titleHeight = 30.0;
        var chartTop = contentTop + titleHeight;
        var chartHeight = contentHeight - chartPadding - 20;
        var chartWidth = contentWidth - chartPadding - 12;
        var chartLeft = contentLeft + chartPadding;

        // 绘制背景和边框
        var rect = new Rect(contentLeft, contentTop, contentWidth, contentHeight);
        var radius = CornerRadius;

        if (hasBorder)
        {
            var pen = new Pen(borderBrush, 1);
            pen.Freeze();
            if (radius > 0)
                dc.DrawRoundedRectangle(bg, pen, rect, radius, radius);
            else
                dc.DrawRectangle(bg, pen, rect);
        }
        else if (bg != null && bg != Brushes.Transparent)
        {
            if (radius > 0)
                dc.DrawRoundedRectangle(bg, null, rect, radius, radius);
            else
                dc.DrawRectangle(bg, null, rect);
        }

        // 裁剪图表区域
        if (radius > 0)
            dc.PushClip(new RectangleGeometry(rect, radius, radius));
        else
            dc.PushClip(new RectangleGeometry(rect));

        // 图表区域背景
        dc.DrawRectangle(ChartBackground, null,
            new Rect(chartLeft, chartTop, chartWidth, chartHeight));

        // 标题
        var titleBrush = new SolidColorBrush(Colors.White);
        var titleFont = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
        dc.DrawText(new FormattedText(
            Title,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            titleFont,
            12,
            titleBrush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip),
            new Point(contentLeft + 12, contentTop + 8));

        // 绘制网格线
        var gridBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
        var gridPen = new Pen(gridBrush, 1) { DashStyle = DashStyles.Dash };

        for (var i = 1; i <= 4; i++)
        {
            var y = chartTop + (chartHeight / 4.0) * i;
            dc.DrawLine(gridPen, new Point(chartLeft, y), new Point(chartLeft + chartWidth, y));
        }

        // Y 轴标签
        var labelBrush = new SolidColorBrush(Colors.Gray);
        var labelFont = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        for (var i = 0; i <= 4; i++)
        {
            var y = chartTop + (chartHeight / 4.0) * i;
            var labelValue = MaxY * (4 - i) / 4.0;
            dc.DrawText(new FormattedText(
                labelValue.ToString("F0"),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                labelFont,
                9,
                labelBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip),
                new Point(contentLeft, y - 6));
        }

        // 绘制数据线
        var points = DataPoints?.ToList();
        if (points != null && points.Count > 1)
        {
            var maxY = MaxY;
            var maxX = Math.Max(MaxX, 1);

            var visiblePoints = points.Count > MaxVisiblePoints
                ? points.Skip(points.Count - MaxVisiblePoints).ToList()
                : points;

            if (visiblePoints.Count > 1)
            {
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    var first = visiblePoints[0];
                    var startX = chartLeft + (first.X / maxX) * chartWidth;
                    var startY = chartTop + chartHeight - (first.Y / maxY) * chartHeight;
                    ctx.BeginFigure(new Point(startX, startY), false, false);

                    for (var i = 1; i < visiblePoints.Count; i++)
                    {
                        var p = visiblePoints[i];
                        var x = chartLeft + (p.X / maxX) * chartWidth;
                        var y = chartTop + chartHeight - (p.Y / maxY) * chartHeight;
                        y = Math.Max(chartTop, Math.Min(chartTop + chartHeight, y));
                        ctx.LineTo(new Point(x, y), true, true);
                    }
                }

                geometry.Freeze();
                var linePen = new Pen(LineBrush, LineThickness);
                linePen.Freeze();
                dc.DrawGeometry(null, linePen, geometry);
            }
        }

        // 进度条
        var progressBottom = contentTop + contentHeight - 8;
        var progressHeight = 4.0;
        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), null,
            new Rect(contentLeft + 12, progressBottom - progressHeight, contentWidth - 24, progressHeight));

        if (ProgressValue > 0)
        {
            dc.DrawRectangle(LineBrush, null,
                new Rect(contentLeft + 12, progressBottom - progressHeight,
                    (contentWidth - 24) * (ProgressValue / 100.0), progressHeight));
        }

        dc.Pop();
    }
}
