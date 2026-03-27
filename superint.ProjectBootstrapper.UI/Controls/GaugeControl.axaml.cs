using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace superint.ProjectBootstrapper.UI.Controls;

public partial class GaugeControl : UserControl
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<GaugeControl, double>(nameof(Value), 0);

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<GaugeControl, string>(nameof(Label), string.Empty);

    public static readonly StyledProperty<IBrush> GaugeColorProperty =
        AvaloniaProperty.Register<GaugeControl, IBrush>(nameof(GaugeColor), Brushes.Green);

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public IBrush GaugeColor
    {
        get => GetValue(GaugeColorProperty);
        set => SetValue(GaugeColorProperty, value);
    }

    public GaugeControl()
    {
        InitializeComponent();
    }
}
