using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

public partial class WizardContainerRegistryStepViewModel : ViewModelBase
{
    private readonly ProjectConfiguration _config;

    public WizardContainerRegistryStepViewModel(ProjectConfiguration config)
    {
        _config = config;
    }

    public bool IsEnabled
    {
        get => _config.CreateContainerRegistryProject;
        set => SetProperty(_config.CreateContainerRegistryProject, value, _config, (c, v) => c.CreateContainerRegistryProject = v);
    }

    public string ProjectName
    {
        get => _config.ContainerRegistryProjectName ?? string.Empty;
        set
        {
            if (SetProperty(_config.ContainerRegistryProjectName, value, _config, (c, v) => c.ContainerRegistryProjectName = v))
            {
                AutoFillPaths();
            }
        }
    }

    public string[] PathPatternOptions { get; } =
    [
        "{project}/{env}/{stack}",
        "{project}/{stack}/{env}",
        "{project}/{stack}"
    ];

    public string PathPattern
    {
        get => _config.ContainerRegistryPathPattern ?? "{project}/{env}/{stack}";
        set
        {
            if (SetProperty(_config.ContainerRegistryPathPattern, value, _config, (c, v) => c.ContainerRegistryPathPattern = v))
            {
                AutoFillPaths();
            }
        }
    }

    // Editable CR paths per stack/env
    public string PathBackendStg
    {
        get => _config.ContainerRegistryPathBackendStg;
        set => SetProperty(_config.ContainerRegistryPathBackendStg, value, _config, (c, v) => c.ContainerRegistryPathBackendStg = v);
    }

    public string PathBackendPrd
    {
        get => _config.ContainerRegistryPathBackendPrd;
        set => SetProperty(_config.ContainerRegistryPathBackendPrd, value, _config, (c, v) => c.ContainerRegistryPathBackendPrd = v);
    }

    public string PathFrontendStg
    {
        get => _config.ContainerRegistryPathFrontendStg;
        set => SetProperty(_config.ContainerRegistryPathFrontendStg, value, _config, (c, v) => c.ContainerRegistryPathFrontendStg = v);
    }

    public string PathFrontendPrd
    {
        get => _config.ContainerRegistryPathFrontendPrd;
        set => SetProperty(_config.ContainerRegistryPathFrontendPrd, value, _config, (c, v) => c.ContainerRegistryPathFrontendPrd = v);
    }

    // Visibility based on project type
    public bool ShowBackendPaths => _config.ProjectType == ProjectType.Backend || _config.ProjectType == ProjectType.Fullstack;
    public bool ShowFrontendPaths => _config.ProjectType == ProjectType.Frontend || _config.ProjectType == ProjectType.Fullstack;

    /// <summary>
    /// Auto-fills paths from ProjectName + PathPattern, then notifies UI
    /// </summary>
    private void AutoFillPaths()
    {
        _config.AutoFillContainerRegistryPaths();
        OnPropertyChanged(nameof(PathBackendStg));
        OnPropertyChanged(nameof(PathBackendPrd));
        OnPropertyChanged(nameof(PathFrontendStg));
        OnPropertyChanged(nameof(PathFrontendPrd));
    }
}
