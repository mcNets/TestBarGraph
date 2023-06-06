using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace TestBarGraph;

public sealed class BarGraphControl : Canvas {

    const int TICK_DIVISIONS = 100;
    const int TEXT_DIVISIONS = 20;

    private Line[]? _topTickMarks;
    private Line[]? _bottomTickMarks;
    private TextBlock[]? _scaleText;

    private SolidColorBrush? _foregroundBrush;
    private LinearGradientBrush? _barBrush;
    private StackPanel? _panelScale;
    private Rectangle? _frame;
    private Rectangle? _bar;

    public BarGraphControl() {
        MinHeight = 60;
        MinWidth = 400;
        InitialitzeComponents();
    }

    private void InitialitzeComponents() {
        // Brushes
        _foregroundBrush = new SolidColorBrush(Colors.White);

        _barBrush = new LinearGradientBrush() {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
        };
        _barBrush.GradientStops.Add(new GradientStop() { Color = ColorHelper.FromArgb(255, 33, 62, 25), Offset = 0 });
        _barBrush.GradientStops.Add(new GradientStop() { Color = ColorHelper.FromArgb(255, 79, 153, 67), Offset = 1 });

        // Bar value
        _bar = new Rectangle() {
            Fill = _barBrush,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        this.Children.Add(_bar);

        // Tick marks
        _topTickMarks = new Line[TICK_DIVISIONS - 1];
        _bottomTickMarks = new Line[TICK_DIVISIONS - 1];
        for (int x = 0; x < TICK_DIVISIONS - 1; x++) {
            _topTickMarks[x] = new Line();
            _topTickMarks[x].Stroke = _foregroundBrush;
            this.Children.Add(_topTickMarks[x]);

            _bottomTickMarks[x] = new Line();
            _bottomTickMarks[x].Stroke = _foregroundBrush;
            this.Children.Add(_bottomTickMarks[x]);
        }

        // Frame rectangle
        _frame = new Rectangle() {
            Stroke = _foregroundBrush,
            StrokeThickness = 1,
            Fill = new SolidColorBrush(Colors.Transparent)
        };
        this.Children.Add(_frame);

        // Panel scale
        _panelScale = new StackPanel() {
            Orientation = Orientation.Horizontal
        };

        _scaleText = new TextBlock[TEXT_DIVISIONS - 1];

        int textValue = 5;
        for (int x = 0; x < TEXT_DIVISIONS - 1; x++) {
            _scaleText[x] = new TextBlock();
            _scaleText[x].Text = textValue.ToString();
            _scaleText[x].VerticalAlignment = VerticalAlignment.Center;
            _scaleText[x].HorizontalAlignment = HorizontalAlignment.Center;
            _scaleText[x].HorizontalTextAlignment = TextAlignment.Center;
            _scaleText[x].Foreground = _foregroundBrush;

            _panelScale.Children.Add(_scaleText[x]);
            textValue += 5;
        }

        this.Children.Add(_panelScale);
    }

    protected override Size MeasureOverride(Size availableSize) {
        Width = double.IsInfinity(availableSize.Width) ? MinWidth : availableSize.Width;
        Height = double.IsInfinity(availableSize.Height) ? MinHeight : availableSize.Height;
        var size = new Size(Width, Height);
        _bar?.Measure(size);
        _frame?.Measure(size);
        for (int x = 0; x < TICK_DIVISIONS - 1; x++) {
            _topTickMarks![x]?.Measure(size);
            _bottomTickMarks![x]?.Measure(size);
        }
        _panelScale!.Measure(size);
        return size;
    }

    protected override Size ArrangeOverride(Size finalSize) {
        Width = double.IsInfinity(finalSize.Width) ? MinWidth : finalSize.Width;
        Height = double.IsInfinity(finalSize.Height) ? MinHeight : finalSize.Height;
        var rect = new Rect(0, 0, Width, Height);
        _bar!.Arrange(rect);
        _frame!.Arrange(rect);
        for (int x = 0; x < TICK_DIVISIONS - 1; x++) {
            _topTickMarks![x]?.Arrange(rect);
            _bottomTickMarks![x]?.Arrange(rect);
        }
        _panelScale!.Arrange(rect);
        ArrangeChilds();
        return new Size(Width, Height);
    }

    private void ArrangeChilds() {
        _bar!.Height = Height;
        _bar!.Width = (Width / 100) * BarValue;

        _frame!.Height = Height;
        _frame!.Width = Width;

        double offsetX = Width / TICK_DIVISIONS;
        double offsetY = Height / 10;

        double posX = offsetX;

        for (int x = 0; x < TICK_DIVISIONS - 1; x++) {
            _topTickMarks![x].X1 = _topTickMarks[x].X2 = posX;
            _topTickMarks[x].Y1 = 0;
            _topTickMarks[x].Y2 = ((x + 1) % 5 == 0) ? offsetY * 2 : offsetY;

            _bottomTickMarks![x].X1 = _bottomTickMarks[x].X2 = posX;
            _bottomTickMarks[x].Y1 = Height - (((x + 1) % 5 == 0) ? offsetY * 2 : offsetY);
            _bottomTickMarks[x].Y2 = Height;

            posX += offsetX;
        }


        double offsetXText = Width / TEXT_DIVISIONS;
        _panelScale!.Height = Height;
        _panelScale!.Width = Width;
        _panelScale!.Padding = new Thickness(offsetXText / 2, 0, 0, 0);

        for (int x = 0; x < TEXT_DIVISIONS - 1; x++) {
            _scaleText![x].Width = offsetXText;
        }
    }

    /// <summary>
    /// Progressbar value
    /// </summary>
    public double BarValue {
        get { return (double)GetValue(BarValueProperty); }
        set {
            if (value < 0.0)
                value = 0.0;
            if (value > 100.0)
                value = 100.0;
            SetValue(BarValueProperty, value);
        }
    }

    /// <summary>
    /// When the value of the progressbar changes the control is invalidated
    /// </summary>
    public static readonly DependencyProperty BarValueProperty =
        DependencyProperty.Register(
            nameof(BarValue),
            typeof(double),
            typeof(BarGraphControl),
            new PropertyMetadata(0.0,  OnBarValueChanged));

    private static void OnBarValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        if (args.NewValue != args.OldValue) {
            (obj as BarGraphControl)?.InvalidateArrange();
        }
    }
}