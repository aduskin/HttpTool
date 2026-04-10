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

    /// <summary>
    /// 数据点
    /// </summary>
    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
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

        var chartPadding = 45.0;
        var titleHeight = 30.0;
        var chartTop = titleHeight;
        var chartHeight = height - chartPadding - 20; // 20 for progress bar
        var chartWidth = width - chartPadding - 12;
        var chartLeft = chartPadding;

        // 背景
        dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x1a, 0x1a, 0x2e)), null,
            new Rect(0, 0, width, height));

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
            new Point(12, 8));

        // 绘制网格线
        var gridBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
        var gridPen = new Pen(gridBrush, 1);
        gridPen.DashStyle = DashStyles.Dash;

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
                new Point(0, y - 6));
        }

        // 绘制数据线
        var points = DataPoints?.ToList();
        if (points != null && points.Count > 0)
        {
            var maxY = MaxY;
            var maxX = Math.Max(MaxX, 1);

            // 采样
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
                var linePen = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0x7b, 0xff)), 1.5);
                linePen.Freeze();
                dc.DrawGeometry(null, linePen, geometry);
            }
        }

        // 进度条背景
        var progressBottom = height - 8;
        var progressHeight = 4.0;
        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), null,
            new Rect(12, progressBottom - progressHeight, width - 24, progressHeight));

        // 进度条前景
        if (ProgressValue > 0)
        {
            var progressBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x7b, 0xff));
            dc.DrawRectangle(progressBrush, null,
                new Rect(12, progressBottom - progressHeight, (width - 24) * (ProgressValue / 100.0), progressHeight));
        }
    }
}
