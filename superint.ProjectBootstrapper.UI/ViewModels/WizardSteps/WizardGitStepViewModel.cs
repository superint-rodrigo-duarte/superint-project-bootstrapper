using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

public partial class WizardGitStepViewModel : ViewModelBase
{
    private readonly ProjectConfiguration _config;

    public WizardGitStepViewModel(ProjectConfiguration config)
    {
        _config = config;
    }

    public bool IsEnabled
    {
        get => _config.CreateGitRepository;
        set
        {
            if (SetProperty(_config.CreateGitRepository, value, _config, (c, v) => c.CreateGitRepository = v))
            {
                OnPropertyChanged(nameof(ShowCreateRepoSection));
                OnPropertyChanged(nameof(ShowExistingRepoSection));
            }
        }
    }

    public string GitNamespace
    {
        get => _config.GitNamespace ?? string.Empty;
        set
        {
            if (SetProperty(_config.GitNamespace, value, _config, (c, v) => c.GitNamespace = v))
            {
                NotifyPreviewChanged();
            }
        }
    }

    // Single repo name (Backend or Frontend only)
    public string GitRepoName
    {
        get => _config.GitRepositoryName ?? _config.GetDefaultGitRepoName();
        set
        {
            if (SetProperty(_config.GitRepositoryName, value, _config, (c, v) => c.GitRepositoryName = v))
            {
                NotifyPreviewChanged();
            }
        }
    }

    // Fullstack: editable repo names
    public string BackendRepoName
    {
        get => _config.BackendRepositoryName;
        set
        {
            if (SetProperty(_config.BackendRepositoryName, value, _config, (c, v) => c.BackendRepositoryName = v))
            {
                OnPropertyChanged(nameof(BackendRepoPreview));
            }
        }
    }

    public string FrontendRepoName
    {
        get => _config.FrontendRepositoryName;
        set
        {
            if (SetProperty(_config.FrontendRepositoryName, value, _config, (c, v) => c.FrontendRepositoryName = v))
            {
                OnPropertyChanged(nameof(FrontendRepoPreview));
            }
        }
    }

    // URLs for existing repos (shown when Git creation is DISABLED)
    public string? GitRepoUrl
    {
        get => _config.GitRepositoryUrl;
        set => SetProperty(_config.GitRepositoryUrl, value, _config, (c, v) => c.GitRepositoryUrl = v);
    }

    public string? BackendGitUrl
    {
        get => _config.BackendGitUrl;
        set => SetProperty(_config.BackendGitUrl, value, _config, (c, v) => c.BackendGitUrl = v);
    }

    public string? FrontendGitUrl
    {
        get => _config.FrontendGitUrl;
        set => SetProperty(_config.FrontendGitUrl, value, _config, (c, v) => c.FrontendGitUrl = v);
    }

    public bool InitializeWithReadme
    {
        get => _config.InitializeGitRepositoryWithReadme;
        set => SetProperty(_config.InitializeGitRepositoryWithReadme, value, _config, (c, v) => c.InitializeGitRepositoryWithReadme = v);
    }

    // Visibility helpers
    public bool IsFullstack => _config.ProjectType == ProjectType.Fullstack;
    public bool IsSingleRepo => _config.ProjectType != ProjectType.Fullstack;

    // Section visibility
    public bool ShowCreateRepoSection => IsEnabled;
    public bool ShowExistingRepoSection => !IsEnabled;

    // Preview: namespace/repo-name
    public string RepoPreview
    {
        get
        {
            var ns = string.IsNullOrWhiteSpace(GitNamespace) ? "org" : GitNamespace;
            return $"{ns}/{GitRepoName}";
        }
    }

    public string BackendRepoPreview
    {
        get
        {
            var ns = string.IsNullOrWhiteSpace(GitNamespace) ? "org" : GitNamespace;
            return $"{ns}/{_config.BackendRepositoryName}";
        }
    }

    public string FrontendRepoPreview
    {
        get
        {
            var ns = string.IsNullOrWhiteSpace(GitNamespace) ? "org" : GitNamespace;
            return $"{ns}/{_config.FrontendRepositoryName}";
        }
    }

    private void NotifyPreviewChanged()
    {
        OnPropertyChanged(nameof(RepoPreview));
        OnPropertyChanged(nameof(BackendRepoPreview));
        OnPropertyChanged(nameof(FrontendRepoPreview));
    }
}
