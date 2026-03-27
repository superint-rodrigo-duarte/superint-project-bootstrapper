using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

public partial class WizardJenkinsStepViewModel : ViewModelBase
{
    private readonly ProjectConfiguration _config;

    public WizardJenkinsStepViewModel(ProjectConfiguration config)
    {
        _config = config;
    }

    public bool IsEnabled
    {
        get => _config.CreateJenkinsPipeline;
        set
        {
            if (SetProperty(_config.CreateJenkinsPipeline, value, _config, (c, v) => c.CreateJenkinsPipeline = v))
            {
                OnPropertyChanged(nameof(ShowGitUrlInfo));
            }
        }
    }

    public string Folder
    {
        get => _config.JenkinsFolder ?? string.Empty;
        set => SetProperty(_config.JenkinsFolder, value, _config, (c, v) => c.JenkinsFolder = v);
    }

    public string ProjectFolderName
    {
        get => _config.JenkinsProjectFolderName ?? _config.ProjectSlug;
        set => SetProperty(_config.JenkinsProjectFolderName, value, _config, (c, v) => c.JenkinsProjectFolderName = v);
    }

    public string BackendPipelineName
    {
        get => _config.JenkinsBackendPipelineName ?? "backend";
        set => SetProperty(_config.JenkinsBackendPipelineName, value, _config, (c, v) => c.JenkinsBackendPipelineName = v);
    }

    public string FrontendPipelineName
    {
        get => _config.JenkinsFrontendPipelineName ?? "frontend";
        set => SetProperty(_config.JenkinsFrontendPipelineName, value, _config, (c, v) => c.JenkinsFrontendPipelineName = v);
    }

    public bool ShowBackendPipeline => _config.ProjectType == ProjectType.Backend || _config.ProjectType == ProjectType.Fullstack;
    public bool ShowFrontendPipeline => _config.ProjectType == ProjectType.Frontend || _config.ProjectType == ProjectType.Fullstack;

    // Git Token Credentials ID (shared, always visible)
    public string GitCredentialsId
    {
        get => _config.JenkinsGitCredentialsId ?? "GitHub-Token-superint-CICD";
        set => SetProperty(_config.JenkinsGitCredentialsId, value, _config, (c, v) => c.JenkinsGitCredentialsId = v);
    }

    public string GitUrlIdPrefix
    {
        get => _config.JenkinsGitUrlIdPrefix ?? "GitHub-URL-";
        set => SetProperty(_config.JenkinsGitUrlIdPrefix, value, _config, (c, v) => c.JenkinsGitUrlIdPrefix = v);
    }

    // Per-stack Git URL credential creation toggles
    public bool CreateGitCredentialBackend
    {
        get => _config.CreateJenkinsGitCredentialBackend;
        set
        {
            if (SetProperty(_config.CreateJenkinsGitCredentialBackend, value, _config, (c, v) => c.CreateJenkinsGitCredentialBackend = v))
            {
                OnPropertyChanged(nameof(ShowBackendCredentialCreate));
                OnPropertyChanged(nameof(ShowBackendCredentialSkip));
            }
        }
    }

    public bool CreateGitCredentialFrontend
    {
        get => _config.CreateJenkinsGitCredentialFrontend;
        set
        {
            if (SetProperty(_config.CreateJenkinsGitCredentialFrontend, value, _config, (c, v) => c.CreateJenkinsGitCredentialFrontend = v))
            {
                OnPropertyChanged(nameof(ShowFrontendCredentialCreate));
                OnPropertyChanged(nameof(ShowFrontendCredentialSkip));
            }
        }
    }

    // Existing credential IDs (when skipping creation)
    public string ExistingGitUrlIdBackend
    {
        get => _config.JenkinsExistingGitUrlIdBackend ?? string.Empty;
        set => SetProperty(_config.JenkinsExistingGitUrlIdBackend, value, _config, (c, v) => c.JenkinsExistingGitUrlIdBackend = v);
    }

    public string ExistingGitUrlIdFrontend
    {
        get => _config.JenkinsExistingGitUrlIdFrontend ?? string.Empty;
        set => SetProperty(_config.JenkinsExistingGitUrlIdFrontend, value, _config, (c, v) => c.JenkinsExistingGitUrlIdFrontend = v);
    }

    // Visibility for create/skip states
    public bool ShowBackendCredentialCreate => CreateGitCredentialBackend;
    public bool ShowBackendCredentialSkip => !CreateGitCredentialBackend;
    public bool ShowFrontendCredentialCreate => CreateGitCredentialFrontend;
    public bool ShowFrontendCredentialSkip => !CreateGitCredentialFrontend;

    // Credential ID previews
    public string BackendCredentialIdPreview => ProjectConfiguration.GetJenkinsGitUrlCredentialId(_config.BackendRepositoryName);
    public string FrontendCredentialIdPreview => ProjectConfiguration.GetJenkinsGitUrlCredentialId(_config.FrontendRepositoryName);
    public string SingleCredentialIdPreview => ProjectConfiguration.GetJenkinsGitUrlCredentialId(_config.GitRepositoryName);

    // Git URL display (read-only, from Git step)
    public string? GitRepoUrl => _config.GitRepositoryUrl;
    public string? BackendGitUrl => _config.BackendGitUrl;
    public string? FrontendGitUrl => _config.FrontendGitUrl;

    // Frontend-only credential block: only for Frontend type (not Fullstack, not Backend)
    public bool ShowFrontendOnlyCredential => _config.ProjectType == ProjectType.Frontend;

    public bool ShowGitUrlInfo => IsEnabled && !_config.CreateGitRepository;
    public bool IsFullstack => _config.ProjectType == ProjectType.Fullstack;
    public bool IsSingleRepo => _config.ProjectType != ProjectType.Fullstack;

    public void Refresh()
    {
        OnPropertyChanged(nameof(ShowGitUrlInfo));
        OnPropertyChanged(nameof(GitRepoUrl));
        OnPropertyChanged(nameof(BackendGitUrl));
        OnPropertyChanged(nameof(FrontendGitUrl));
        OnPropertyChanged(nameof(BackendCredentialIdPreview));
        OnPropertyChanged(nameof(FrontendCredentialIdPreview));
        OnPropertyChanged(nameof(SingleCredentialIdPreview));
        OnPropertyChanged(nameof(ShowBackendCredentialCreate));
        OnPropertyChanged(nameof(ShowBackendCredentialSkip));
        OnPropertyChanged(nameof(ShowFrontendCredentialCreate));
        OnPropertyChanged(nameof(ShowFrontendCredentialSkip));
        OnPropertyChanged(nameof(ShowBackendPipeline));
        OnPropertyChanged(nameof(ShowFrontendPipeline));
        OnPropertyChanged(nameof(IsFullstack));
        OnPropertyChanged(nameof(IsSingleRepo));
        OnPropertyChanged(nameof(ShowFrontendOnlyCredential));
    }
}
