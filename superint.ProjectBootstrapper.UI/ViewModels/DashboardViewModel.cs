using CommunityToolkit.Mvvm.ComponentModel;
using superint.ProjectBootstrapper.UI.Constants;

namespace superint.ProjectBootstrapper.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _stgStatus = UIStrings.Status.WaitingConnection;

    [ObservableProperty]
    private string _prdStatus = UIStrings.Status.WaitingConnection;

    [ObservableProperty]
    private bool _isLoading;

    // STG Metrics (placeholders)
    [ObservableProperty]
    private double _stgCpuUsage;

    [ObservableProperty]
    private double _stgMemoryUsage;

    [ObservableProperty]
    private double _stgDiskUsage;

    [ObservableProperty]
    private string _stgUptime = "--";

    // STG Details (for subtle display below gauges)
    [ObservableProperty]
    private string _stgCpuDetails = "-- / -- cores";

    [ObservableProperty]
    private string _stgMemoryDetails = "-- / -- GB";

    [ObservableProperty]
    private string _stgDiskDetails = "-- / -- GB";

    // PRD Metrics (placeholders)
    [ObservableProperty]
    private double _prdCpuUsage;

    [ObservableProperty]
    private double _prdMemoryUsage;

    [ObservableProperty]
    private double _prdDiskUsage;

    [ObservableProperty]
    private string _prdUptime = "--";

    // PRD Details (for subtle display below gauges)
    [ObservableProperty]
    private string _prdCpuDetails = "-- / -- cores";

    [ObservableProperty]
    private string _prdMemoryDetails = "-- / -- GB";

    [ObservableProperty]
    private string _prdDiskDetails = "-- / -- GB";

    public DashboardViewModel()
    {
        // Placeholder - lógica será implementada depois
    }
}
