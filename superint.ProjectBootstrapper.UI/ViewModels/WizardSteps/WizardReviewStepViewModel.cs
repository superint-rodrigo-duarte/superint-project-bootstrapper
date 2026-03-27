using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

public partial class WizardReviewStepViewModel : ViewModelBase
{
    private readonly ProjectConfiguration _config;

    public WizardReviewStepViewModel(ProjectConfiguration config)
    {
        _config = config;
    }

    public ProjectConfiguration Configuration => _config;

    // Enabled/Disabled flags
    public bool WillCreateGit => _config.CreateGitRepository;
    public bool WillCreateRegistry => _config.CreateContainerRegistryProject;
    public bool WillCreateJenkins => _config.CreateJenkinsPipeline;
    public bool WillCreateDatabase => _config.CreateDatabaseUser;
    public bool WillCreateDocker => _config.GenerateDockerCompose;

    // Status text helpers — "Create" / "Skip"
    public string GitStatusText => WillCreateGit ? "Create" : "Skip";
    public string RegistryStatusText => WillCreateRegistry ? "Create" : "Skip";
    public string JenkinsStatusText => WillCreateJenkins ? "Create" : "Skip";
    public string DatabaseStatusText => WillCreateDatabase ? "Create" : "Skip";
    public string DockerStatusText => WillCreateDocker ? "Create" : "Skip";

    // Git details
    public string GitNamespace => _config.GitNamespace ?? string.Empty;
    public string GitRepoName => _config.GitRepositoryName;
    public string BackendRepoName => _config.BackendRepositoryName;
    public string FrontendRepoName => _config.FrontendRepositoryName;
    public bool IsFullstack => _config.ProjectType == ProjectType.Fullstack;
    public bool IsSingleRepo => _config.ProjectType != ProjectType.Fullstack;

    // Container Registry details
    public string RegistryProjectName => _config.ContainerRegistryProjectName;
    public string RegistryPathPattern => _config.ContainerRegistryPathPattern;
    public string RegistryPathBackendStg => _config.ContainerRegistryPathBackendStg;
    public string RegistryPathBackendPrd => _config.ContainerRegistryPathBackendPrd;
    public string RegistryPathFrontendStg => _config.ContainerRegistryPathFrontendStg;
    public string RegistryPathFrontendPrd => _config.ContainerRegistryPathFrontendPrd;
    public bool ShowBackendPaths => _config.ProjectType == ProjectType.Backend || _config.ProjectType == ProjectType.Fullstack;
    public bool ShowFrontendPaths => _config.ProjectType == ProjectType.Frontend || _config.ProjectType == ProjectType.Fullstack;

    // Jenkins details
    public string JenkinsFolderPath
    {
        get
        {
            var basePath = string.IsNullOrEmpty(_config.JenkinsFolder) ? "" : $"{_config.JenkinsFolder}/";
            return $"{basePath}{_config.JenkinsProjectFolderName}/";
        }
    }
    public string JenkinsBackendPipeline => _config.JenkinsBackendPipelineName;
    public string JenkinsFrontendPipeline => _config.JenkinsFrontendPipelineName;
    public bool ShowBackendPipeline => _config.ProjectType == ProjectType.Backend || _config.ProjectType == ProjectType.Fullstack;
    public bool ShowFrontendPipeline => _config.ProjectType == ProjectType.Frontend || _config.ProjectType == ProjectType.Fullstack;

    // Jenkins Git Credential status per stack
    public string JenkinsGitCredStatusBackend => _config.CreateJenkinsGitCredentialBackend
        ? "Create"
        : $"Skip (use: {_config.JenkinsExistingGitUrlIdBackend ?? "not set"})";
    public string JenkinsGitCredStatusFrontend => _config.CreateJenkinsGitCredentialFrontend
        ? "Create"
        : $"Skip (use: {_config.JenkinsExistingGitUrlIdFrontend ?? "not set"})";

    // Database details
    public string DatabaseUserStg => _config.DatabaseUserStg;
    public string DatabaseUserPrd => _config.DatabaseUserPrd;
    public bool CreateInStg => _config.CreateDatabaseUserStg;
    public bool CreateInPrd => _config.CreateDatabaseUserPrd;

    // Docker details
    public string DeployFolderName => _config.DeployFolderName;
    public string ContainerNameBackend => _config.ContainerNameBackend;
    public string ContainerNameFrontend => _config.ContainerNameFrontend;
    public string ReverseProxyHostStg => _config.ReverseProxyHostStg;
    public string ReverseProxyHostPrd => _config.ReverseProxyHostPrd;
    public string ReverseProxyPathPrefixStg => _config.ReverseProxyPathPrefixStg;
    public string ReverseProxyPathPrefixPrd => _config.ReverseProxyPathPrefixPrd;
    public bool DeployToStg => _config.DeployDockerComposeStg;
    public bool DeployToPrd => _config.DeployDockerComposePrd;
    public bool CopyToLocalStg => _config.CopyDockerComposeLocalStg;
    public bool CopyToLocalPrd => _config.CopyDockerComposeLocalPrd;
    public bool ShowFrontendContainers => _config.ProjectType == ProjectType.Frontend || _config.ProjectType == ProjectType.Fullstack;
    public bool ShowBackendContainers => _config.ProjectType == ProjectType.Backend || _config.ProjectType == ProjectType.Fullstack;

    public string DockerDeployTargets
    {
        get
        {
            var targets = new List<string>();
            if (_config.DeployDockerComposeStg) targets.Add("STG");
            if (_config.DeployDockerComposePrd) targets.Add("PRD");
            if (_config.CopyDockerComposeLocalStg) targets.Add("Local STG");
            if (_config.CopyDockerComposeLocalPrd) targets.Add("Local PRD");
            return targets.Count > 0 ? string.Join(", ", targets) : "None";
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(string.Empty);
    }
}
