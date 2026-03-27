using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.DTO;

public sealed class ProjectConfiguration
{
    private string? _backendRepositoryName;
    private string? _frontendRepositoryName;

    #region Informações Básicas
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectSlug => ProjectName.ToLowerInvariant().Replace(" ", "-").Replace("_", "-");
    public ProjectType ProjectType { get; set; } = ProjectType.Backend;
    public string Description { get; set; } = string.Empty;
    #endregion

    #region Linguagens
    public ProgrammingLanguage FrontendProgrammingLanguage { get; set; } = ProgrammingLanguage.NodeJS;
    public ProgrammingLanguage BackendProgrammingLanguage { get; set; } = ProgrammingLanguage.NodeJS;
    public string? DotNetProjectName { get; set; }
    #endregion

    #region Git
    public bool CreateGitRepository { get; set; } = true;
    public bool InitializeGitRepositoryWithReadme { get; set; } = false;
    public string? GitNamespace { get; set; }
    public string GitRepositoryName { get; set; } = string.Empty;

    public string BackendRepositoryName
    {
        get => _backendRepositoryName ?? (ProjectType == ProjectType.Fullstack ? $"{GitRepositoryName}-backend" : GitRepositoryName);
        set => _backendRepositoryName = value;
    }

    public string FrontendRepositoryName
    {
        get => _frontendRepositoryName ?? (ProjectType == ProjectType.Fullstack ? $"{GitRepositoryName}-frontend" : GitRepositoryName);
        set => _frontendRepositoryName = value;
    }

    public string? GitRepositoryUrl { get; set; }
    public string? BackendGitUrl { get; set; }
    public string? FrontendGitUrl { get; set; }

    public void ResetRepositoryNames()
    {
        _backendRepositoryName = null;
        _frontendRepositoryName = null;
    }
    #endregion

    #region Container Registry
    public bool CreateContainerRegistryProject { get; set; } = true;
    public string ContainerRegistryProjectName { get; set; } = string.Empty;
    public string ContainerRegistryPathPattern { get; set; } = "{project}/{env}/{stack}";
    public string ContainerRegistryPathBackendStg { get; set; } = string.Empty;
    public string ContainerRegistryPathBackendPrd { get; set; } = string.Empty;
    public string ContainerRegistryPathFrontendStg { get; set; } = string.Empty;
    public string ContainerRegistryPathFrontendPrd { get; set; } = string.Empty;

    public string GetContainerRegistryPath(string environment, string stack)
    {
        return ContainerRegistryPathPattern.Replace("{project}", ContainerRegistryProjectName)
                                           .Replace("{env}", environment.ToLowerInvariant())
                                           .Replace("{stack}", stack.ToLowerInvariant());
    }

    public void AutoFillContainerRegistryPaths()
    {
        ContainerRegistryPathBackendStg = GetContainerRegistryPath("staging", "backend");
        ContainerRegistryPathBackendPrd = GetContainerRegistryPath("main", "backend");
        ContainerRegistryPathFrontendStg = GetContainerRegistryPath("staging", "frontend");
        ContainerRegistryPathFrontendPrd = GetContainerRegistryPath("main", "frontend");
    }
    #endregion

    #region Jenkins
    public bool CreateJenkinsPipeline { get; set; } = true;
    public bool CreateJenkinsGitCredentialBackend { get; set; } = true;
    public bool CreateJenkinsGitCredentialFrontend { get; set; } = true;
    public string? JenkinsExistingGitUrlIdBackend { get; set; }
    public string? JenkinsExistingGitUrlIdFrontend { get; set; }
    public string? JenkinsFolder { get; set; }
    public string JenkinsProjectFolderName { get; set; } = string.Empty;
    public string JenkinsBackendPipelineName { get; set; } = "backend";
    public string JenkinsFrontendPipelineName { get; set; } = "frontend";
    public string JenkinsGitCredentialsId { get; set; } = "";
    public string JenkinsGitUrlIdPrefix { get; set; } = "";

    public static string GetJenkinsGitUrlCredentialId(string repoName) => $"GitHub-URL-{repoName}";
    public static string GetJenkinsGitUrlCredentialName(string repoName) => $"GitHub URL - {repoName}";

    public string GetEffectiveGitUrlId(PipelineStackType stackType, string repoName)
    {
        var isCreating = stackType == PipelineStackType.Backend ? CreateJenkinsGitCredentialBackend : CreateJenkinsGitCredentialFrontend;

        if (isCreating)
            return GetJenkinsGitUrlCredentialId(repoName);

        var existingId = stackType == PipelineStackType.Backend ? JenkinsExistingGitUrlIdBackend : JenkinsExistingGitUrlIdFrontend;

        return existingId ?? GetJenkinsGitUrlCredentialId(repoName);
    }
    #endregion

    #region Database
    public bool CreateDatabaseUser { get; set; } = true;
    public bool CreateDatabaseUserStg { get; set; } = true;
    public bool CreateDatabaseUserPrd { get; set; } = true;
    public string DatabaseUserStg { get; set; } = string.Empty;
    public string? DatabasePasswordStg { get; set; }
    public string DatabaseUserPrd { get; set; } = string.Empty;
    public string? DatabasePasswordPrd { get; set; }
    #endregion

    #region Docker Compose
    public bool GenerateDockerCompose { get; set; } = true;
    public bool DeployDockerCompose { get; set; } = true;
    public bool DeployDockerComposeStg { get; set; } = true;
    public bool DeployDockerComposePrd { get; set; } = true;
    public bool CopyDockerComposeLocalStg { get; set; } = false;
    public bool CopyDockerComposeLocalPrd { get; set; } = false;
    public string DeployFolderName { get; set; } = string.Empty;
    public string ReverseProxyHostStg { get; set; } = "";
    public string ReverseProxyHostPrd { get; set; } = "";
    public string ReverseProxyPathPrefixStg { get; set; } = string.Empty;
    public string ReverseProxyPathPrefixPrd { get; set; } = string.Empty;
    public string ReverseProxyPathPrefixApiStg { get; set; } = string.Empty;
    public string ReverseProxyPathPrefixApiPrd { get; set; } = string.Empty;
    public string? LocalDeployPathStg { get; set; }
    public string? LocalDeployPathPrd { get; set; }
    public string ContainerNameFrontend { get; set; } = string.Empty;
    public string ContainerNameBackend { get; set; } = string.Empty;
    public bool UseStripPrefixFrontendStg { get; set; } = false;
    public bool UseStripPrefixBackendStg { get; set; } = true;
    public bool UseStripPrefixFrontendPrd { get; set; } = false;
    public bool UseStripPrefixBackendPrd { get; set; } = true;

    public string GetDefaultContainerNameFrontend() => $"{ProjectSlug}-frontend";
    public string GetDefaultContainerNameBackend() => $"{ProjectSlug}-backend";

    public string GetDefaultReverseProxyPathPrefix()
    {
        var slug = ProjectSlug;

        if (slug.StartsWith("superint-", StringComparison.OrdinalIgnoreCase))
            slug = slug[9..];

        return $"/{slug}";
    }
    #endregion

    #region Computed - Ports
    public static int FrontendPort => 3000;
    public int BackendPort => BackendProgrammingLanguage == ProgrammingLanguage.CSharp ? 8080 : 3001;
    #endregion

    #region Helpers
    public string GetDefaultGitRepoName() => ProjectSlug;
    public string GetDefaultContainerRegistryProjectName() => ProjectSlug;
    public string GetDefaultDatabaseUser() => $"{ProjectSlug.Replace("-", "_")}-app";
    public string GetDefaultDeployFolderName() => ProjectSlug;
    #endregion
}