using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.UI.Collections;
using superint.ProjectBootstrapper.UI.Constants;
using superint.ProjectBootstrapper.UI.Models;

namespace superint.ProjectBootstrapper.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentSection;

    [ObservableProperty]
    private string _selectedSectionKey = "dashboard";

    [ObservableProperty]
    private object? _currentSectionView;

    [ObservableProperty]
    private LimitedObservableCollection<LogEntry> _logEntries = new(500);

    public LimitedObservableCollection<string> LogMessages { get; } = new(500);

    // Section ViewModels (injected via DI)
    public DashboardViewModel DashboardSection { get; }
    public SecretsViewModel SecretsSection { get; }
    public TestConnectionsViewModel TestConnectionsSection { get; }
    public BootstrapWizardViewModel BootstrapWizardSection { get; }

    public MainViewModel(
        ApplicationSettings appSettings,
        DashboardViewModel dashboardViewModel,
        SecretsViewModel secretsViewModel,
        TestConnectionsViewModel testConnectionsViewModel,
        BootstrapWizardViewModel bootstrapWizardViewModel)
    {
        // Inject section ViewModels
        DashboardSection = dashboardViewModel;
        SecretsSection = secretsViewModel;
        TestConnectionsSection = testConnectionsViewModel;
        BootstrapWizardSection = bootstrapWizardViewModel;

        // Set default section
        CurrentSection = DashboardSection;
        CurrentSectionView = DashboardSection;

        // Welcome message
        AddLog(UIStrings.Messages.AppStarted, LogLevel.Info);
    }

    // Design-time constructor
    public MainViewModel() : this(
        new ApplicationSettings(),
        new DashboardViewModel(),
        new SecretsViewModel(),
        new TestConnectionsViewModel(),
        new BootstrapWizardViewModel())
    {
    }

    partial void OnSelectedSectionKeyChanged(string value)
    {
        var section = value switch
        {
            "dashboard" => (ViewModelBase)DashboardSection,
            "secrets" => SecretsSection,
            "connections" => TestConnectionsSection,
            "bootstrap" => BootstrapWizardSection,
            _ => DashboardSection
        };
        CurrentSection = section;
        CurrentSectionView = section;
    }

    [RelayCommand]
    private void NavigateToSection(string sectionName)
    {
        SelectedSectionKey = sectionName;
    }

    public void AddLog(string message, LogLevel level)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var prefix = level switch
        {
            LogLevel.Info => "[INFO]",
            LogLevel.Success => "[OK]",
            LogLevel.Warning => "[WARN]",
            LogLevel.Error => "[ERROR]",
            _ => "[INFO]"
        };

        LogEntries.Add(new LogEntry
        {
            Timestamp = DateTime.Now,
            Message = message,
            Level = level
        });

        LogMessages.Add($"{timestamp} {prefix} {message}");
    }
}
