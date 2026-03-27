namespace superint.ProjectBootstrapper.UI.Models;

/// <summary>
/// Enum que define os steps do wizard
/// </summary>
public enum WizardStep
{
    BasicInfo = 0,
    Git = 1,
    ContainerRegistry = 2,
    Jenkins = 3,
    Database = 4,
    Docker = 5,
    Review = 6
}
