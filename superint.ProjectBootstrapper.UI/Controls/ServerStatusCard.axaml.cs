using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using superint.ProjectBootstrapper.UI.Constants;

namespace superint.ProjectBootstrapper.UI.Controls;

public partial class ServerStatusCard : UserControl
{
    private bool _isLoaded;

    public static readonly StyledProperty<string> ServerNameProperty =
        AvaloniaProperty.Register<ServerStatusCard, string>(nameof(ServerName), "Server");

    public static readonly StyledProperty<string> ServerTitleProperty =
        AvaloniaProperty.Register<ServerStatusCard, string>(nameof(ServerTitle), "Server");

    public static readonly StyledProperty<IBrush> BadgeColorProperty =
        AvaloniaProperty.Register<ServerStatusCard, IBrush>(nameof(BadgeColor), Brushes.Gray);

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<ServerStatusCard, string>(nameof(Status), UIStrings.Status.WaitingConnection);

    public static readonly StyledProperty<double> CpuUsageProperty =
        AvaloniaProperty.Register<ServerStatusCard, double>(nameof(CpuUsage));

    public static readonly StyledProperty<double> MemoryUsageProperty =
        AvaloniaProperty.Register<ServerStatusCard, double>(nameof(MemoryUsage));

    public static readonly StyledProperty<double> DiskUsageProperty =
        AvaloniaProperty.Register<ServerStatusCard, double>(nameof(DiskUsage));

    public static readonly StyledProperty<string> CpuDetailsProperty =
        AvaloniaProperty.Register<ServerStatusCard, string>(nameof(CpuDetails), "-- / -- cores");

    public static readonly StyledProperty<string> MemoryDetailsProperty =
        AvaloniaProperty.Register<ServerStatusCard, string>(nameof(MemoryDetails), "-- / -- GB");

    public static readonly StyledProperty<string> DiskDetailsProperty =
        AvaloniaProperty.Register<ServerStatusCard, string>(nameof(DiskDetails), "-- / -- GB");

    public static readonly StyledProperty<string> UptimeProperty =
        AvaloniaProperty.Register<ServerStatusCard, string>(nameof(Uptime), "--");

    public string ServerName
    {
        get => GetValue(ServerNameProperty);
        set => SetValue(ServerNameProperty, value);
    }

    public string ServerTitle
    {
        get => GetValue(ServerTitleProperty);
        set => SetValue(ServerTitleProperty, value);
    }

    public IBrush BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public double CpuUsage
    {
        get => GetValue(CpuUsageProperty);
        set => SetValue(CpuUsageProperty, value);
    }

    public double MemoryUsage
    {
        get => GetValue(MemoryUsageProperty);
        set => SetValue(MemoryUsageProperty, value);
    }

    public double DiskUsage
    {
        get => GetValue(DiskUsageProperty);
        set => SetValue(DiskUsageProperty, value);
    }

    public string CpuDetails
    {
        get => GetValue(CpuDetailsProperty);
        set => SetValue(CpuDetailsProperty, value);
    }

    public string MemoryDetails
    {
        get => GetValue(MemoryDetailsProperty);
        set => SetValue(MemoryDetailsProperty, value);
    }

    public string DiskDetails
    {
        get => GetValue(DiskDetailsProperty);
        set => SetValue(DiskDetailsProperty, value);
    }

    public string Uptime
    {
        get => GetValue(UptimeProperty);
        set => SetValue(UptimeProperty, value);
    }

    public ServerStatusCard()
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

        if (_isLoaded)
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        var badgeBorder = this.FindControl<Border>("BadgeBorder");
        var badgeText = this.FindControl<TextBlock>("BadgeText");
        var titleText = this.FindControl<TextBlock>("TitleText");
        var statusText = this.FindControl<TextBlock>("StatusText");
        var cpuGauge = this.FindControl<GaugeDisplay>("CpuGauge");
        var memoryGauge = this.FindControl<GaugeDisplay>("MemoryGauge");
        var diskGauge = this.FindControl<GaugeDisplay>("DiskGauge");
        var cpuDetailsText = this.FindControl<TextBlock>("CpuDetailsText");
        var memoryDetailsText = this.FindControl<TextBlock>("MemoryDetailsText");
        var diskDetailsText = this.FindControl<TextBlock>("DiskDetailsText");
        var uptimeText = this.FindControl<TextBlock>("UptimeText");

        if (badgeBorder != null)
            badgeBorder.Background = BadgeColor;

        if (badgeText != null)
            badgeText.Text = ServerName;

        if (titleText != null)
            titleText.Text = ServerTitle;

        if (statusText != null)
            statusText.Text = Status;

        if (cpuGauge != null)
            cpuGauge.Value = CpuUsage;

        if (memoryGauge != null)
            memoryGauge.Value = MemoryUsage;

        if (diskGauge != null)
            diskGauge.Value = DiskUsage;

        if (cpuDetailsText != null)
            cpuDetailsText.Text = CpuDetails;

        if (memoryDetailsText != null)
            memoryDetailsText.Text = MemoryDetails;

        if (diskDetailsText != null)
            diskDetailsText.Text = DiskDetails;

        if (uptimeText != null)
            uptimeText.Text = Uptime;
    }
}
