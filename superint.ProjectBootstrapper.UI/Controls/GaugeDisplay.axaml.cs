using Avalonia;
using Avalonia.Controls;

namespace superint.ProjectBootstrapper.UI.Controls;

public partial class GaugeDisplay : UserControl
{
    private bool _isLoaded;

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<GaugeDisplay, double>(nameof(Value));

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<GaugeDisplay, string>(nameof(Label), "Label");

    public static readonly StyledProperty<string> UnitProperty =
        AvaloniaProperty.Register<GaugeDisplay, string>(nameof(Unit), "%");

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

    public string Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public GaugeDisplay()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _isLoaded = true;
        UpdateDisplay();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_isLoaded &&
            (change.Property == ValueProperty ||
             change.Property == LabelProperty ||
             change.Property == UnitProperty))
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        var valueText = this.FindControl<TextBlock>("ValueText");
        var unitText = this.FindControl<TextBlock>("UnitText");
        var labelText = this.FindControl<TextBlock>("LabelText");

        if (valueText != null)
            valueText.Text = Value.ToString("F0");

        if (unitText != null)
            unitText.Text = Unit;

        if (labelText != null)
            labelText.Text = Label;
    }
}
