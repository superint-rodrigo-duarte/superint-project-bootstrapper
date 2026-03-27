using CommunityToolkit.Mvvm.ComponentModel;

namespace superint.ProjectBootstrapper.UI.Models;

/// <summary>
/// Resultado de uma tarefa do bootstrap para exibicao na UI
/// </summary>
public partial class BootstrapTaskResult : ObservableObject
{
    [ObservableProperty]
    private string _taskName = string.Empty;

    [ObservableProperty]
    private BootstrapTaskStatus _status = BootstrapTaskStatus.Pending;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _details;

    [ObservableProperty]
    private bool _isEnabled = true;
}

/// <summary>
/// Status de uma tarefa do bootstrap
/// </summary>
public enum BootstrapTaskStatus
{
    Pending,
    Running,
    Success,
    Failed,
    Skipped
}
