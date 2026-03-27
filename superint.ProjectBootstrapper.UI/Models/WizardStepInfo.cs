using CommunityToolkit.Mvvm.ComponentModel;

namespace superint.ProjectBootstrapper.UI.Models;

/// <summary>
/// Informacoes de um step do wizard para exibicao na UI
/// </summary>
public partial class WizardStepInfo : ObservableObject
{
    [ObservableProperty]
    private WizardStep _stepType;

    [ObservableProperty]
    private int _displayNumber;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isLast;

    /// <summary>
    /// Index do step (baseado no enum)
    /// </summary>
    public int Index => (int)StepType;
}
