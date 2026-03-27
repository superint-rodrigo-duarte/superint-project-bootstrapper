using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

public partial class WizardBasicInfoStepViewModel : ViewModelBase
{
    private readonly ProjectConfiguration _config;

    public WizardBasicInfoStepViewModel(ProjectConfiguration config)
    {
        _config = config;
    }

    public string ProjectName
    {
        get => _config.ProjectName;
        set => SetProperty(_config.ProjectName, value, _config, (c, v) => c.ProjectName = v);
    }

    public string Description
    {
        get => _config.Description;
        set => SetProperty(_config.Description, value, _config, (c, v) => c.Description = v);
    }

    public ProjectType Type
    {
        get => _config.ProjectType;
        set
        {
            SetProperty(_config.ProjectType, value, _config, (c, v) => c.ProjectType = v);
            OnPropertyChanged(nameof(ShowBackendOptions));
            OnPropertyChanged(nameof(ShowFrontendOptions));
            OnPropertyChanged(nameof(ShowDotNetOptions));
        }
    }

    public ProgrammingLanguage BackendLanguage
    {
        get => _config.BackendProgrammingLanguage;
        set
        {
            SetProperty(_config.BackendProgrammingLanguage, value, _config, (c, v) => c.BackendProgrammingLanguage = v);
            OnPropertyChanged(nameof(ShowDotNetOptions));
        }
    }

    public ProgrammingLanguage FrontendLanguage
    {
        get => _config.FrontendProgrammingLanguage;
        set => SetProperty(_config.FrontendProgrammingLanguage, value, _config, (c, v) => c.FrontendProgrammingLanguage = v);
    }

    public string? DotNetProjectName
    {
        get => _config.DotNetProjectName;
        set => SetProperty(_config.DotNetProjectName, value, _config, (c, v) => c.DotNetProjectName = v);
    }

    public bool ShowBackendOptions => Type == ProjectType.Backend || Type == ProjectType.Fullstack;
    public bool ShowFrontendOptions => Type == ProjectType.Frontend || Type == ProjectType.Fullstack;
    public bool ShowDotNetOptions => ShowBackendOptions && BackendLanguage == ProgrammingLanguage.CSharp;

    public IEnumerable<ProjectType> ProjectTypes => Enum.GetValues<ProjectType>();
    public IEnumerable<ProgrammingLanguage> Languages => Enum.GetValues<ProgrammingLanguage>();
}
