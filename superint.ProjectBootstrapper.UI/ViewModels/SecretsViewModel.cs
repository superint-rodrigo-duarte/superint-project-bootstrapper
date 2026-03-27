using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using superint.ProjectBootstrapper.DTO.Configuration;

namespace superint.ProjectBootstrapper.UI.ViewModels;

public partial class SecretsViewModel : ViewModelBase
{
    private readonly ApplicationSettings _appSettings;
    private static readonly string LocalSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");

    // Git Settings
    [ObservableProperty]
    private string _gitBaseUrl = string.Empty;

    [ObservableProperty]
    private string _gitApiToken = string.Empty;

    [ObservableProperty]
    private string _gitDefaultNamespace = string.Empty;

    [ObservableProperty]
    private string _gitProvider = string.Empty;

    // Container Registry Settings
    [ObservableProperty]
    private string _registryUrl = string.Empty;

    [ObservableProperty]
    private string _registryUsername = string.Empty;

    [ObservableProperty]
    private string _registryPassword = string.Empty;

    // Container Registry - Retention Policy
    [ObservableProperty]
    private bool _retentionPolicyEnabled = true;

    [ObservableProperty]
    private int _retentionPolicyRetainCount = 5;

    [ObservableProperty]
    private string _retentionPolicyRepositoryPattern = "**";

    [ObservableProperty]
    private string _retentionPolicyTagPattern = "**";

    [ObservableProperty]
    private bool _retentionPolicyIncludeUntagged = true;

    // Container Registry - Project Admin Users (comma-separated)
    [ObservableProperty]
    private string _projectAdminUsers = string.Empty;

    // Jenkins Settings
    [ObservableProperty]
    private string _jenkinsBaseUrl = string.Empty;

    [ObservableProperty]
    private string _jenkinsUsername = string.Empty;

    [ObservableProperty]
    private string _jenkinsApiToken = string.Empty;

    [ObservableProperty]
    private string _jenkinsDefaultFolder = string.Empty;

    [ObservableProperty]
    private int _jenkinsMaxBuildsToKeep = 5;

    // Jenkins Environments (comma-separated)
    [ObservableProperty]
    private string _jenkinsEnvironments = string.Empty;

    [ObservableProperty]
    private string _jenkinsCredentialsId = string.Empty;

    // Server STG Settings
    [ObservableProperty]
    private string _serverStgHost = string.Empty;

    [ObservableProperty]
    private int _serverStgPort = 22;

    [ObservableProperty]
    private string _serverStgUsername = string.Empty;

    [ObservableProperty]
    private string _serverStgPassword = string.Empty;

    // Server PRD Settings
    [ObservableProperty]
    private string _serverPrdHost = string.Empty;

    [ObservableProperty]
    private int _serverPrdPort = 22;

    [ObservableProperty]
    private string _serverPrdUsername = string.Empty;

    [ObservableProperty]
    private string _serverPrdPassword = string.Empty;

    // PostgreSQL STG Settings
    [ObservableProperty]
    private string _postgresStgContainerName = string.Empty;

    [ObservableProperty]
    private string _postgresStgAdminUser = string.Empty;

    [ObservableProperty]
    private string _postgresStgAdminPassword = string.Empty;

    // PostgreSQL PRD Settings
    [ObservableProperty]
    private string _postgresPrdContainerName = string.Empty;

    [ObservableProperty]
    private string _postgresPrdAdminUser = string.Empty;

    [ObservableProperty]
    private string _postgresPrdAdminPassword = string.Empty;

    // Deployment Settings
    [ObservableProperty]
    private string _deploymentBasePath = string.Empty;

    [ObservableProperty]
    private string _deploymentNetworkName = string.Empty;

    [ObservableProperty]
    private string _deploymentReverseProxyHostStaging = string.Empty;

    [ObservableProperty]
    private string _deploymentReverseProxyHostProduction = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    public SecretsViewModel(ApplicationSettings appSettings)
    {
        _appSettings = appSettings;
        LoadFromAppSettings();
    }

    // Design-time constructor
    public SecretsViewModel()
    {
        _appSettings = new ApplicationSettings();
    }

    private void LoadFromAppSettings()
    {
        // Git
        GitProvider = _appSettings.Git.Provider;
        GitBaseUrl = _appSettings.Git.BaseUrl;
        GitApiToken = _appSettings.Git.ApiToken;
        GitDefaultNamespace = _appSettings.Git.DefaultNamespace;

        // Container Registry
        RegistryUrl = _appSettings.ContainerRegistry.Url;
        RegistryUsername = _appSettings.ContainerRegistry.Username;
        RegistryPassword = _appSettings.ContainerRegistry.Password;
        RetentionPolicyEnabled = _appSettings.ContainerRegistry.RetentionPolicy.Enabled;
        RetentionPolicyRetainCount = _appSettings.ContainerRegistry.RetentionPolicy.RetainCount;
        RetentionPolicyRepositoryPattern = _appSettings.ContainerRegistry.RetentionPolicy.RepositoryPattern;
        RetentionPolicyTagPattern = _appSettings.ContainerRegistry.RetentionPolicy.TagPattern;
        RetentionPolicyIncludeUntagged = _appSettings.ContainerRegistry.RetentionPolicy.IncludeUntagged;
        ProjectAdminUsers = string.Join(", ", _appSettings.ContainerRegistry.ProjectAdminUsers);

        // Jenkins
        JenkinsBaseUrl = _appSettings.Jenkins.BaseUrl;
        JenkinsUsername = _appSettings.Jenkins.Username;
        JenkinsApiToken = _appSettings.Jenkins.ApiToken;
        JenkinsDefaultFolder = _appSettings.Jenkins.DefaultFolder;
        JenkinsMaxBuildsToKeep = _appSettings.Jenkins.MaxBuildsToKeep;
        JenkinsEnvironments = string.Join(", ", _appSettings.Jenkins.Environments);
        JenkinsCredentialsId = _appSettings.Jenkins.CredentialsId;

        // Servers STG
        ServerStgHost = _appSettings.Servers.Stg.Host;
        ServerStgPort = _appSettings.Servers.Stg.Port;
        ServerStgUsername = _appSettings.Servers.Stg.Username;
        ServerStgPassword = _appSettings.Servers.Stg.Password ?? string.Empty;

        // Servers PRD
        ServerPrdHost = _appSettings.Servers.Prd.Host;
        ServerPrdPort = _appSettings.Servers.Prd.Port;
        ServerPrdUsername = _appSettings.Servers.Prd.Username;
        ServerPrdPassword = _appSettings.Servers.Prd.Password ?? string.Empty;

        // PostgreSQL STG
        PostgresStgContainerName = _appSettings.DatabaseContainer.Stg.ContainerName;
        PostgresStgAdminUser = _appSettings.DatabaseContainer.Stg.AdminUser;
        PostgresStgAdminPassword = _appSettings.DatabaseContainer.Stg.AdminPassword;

        // PostgreSQL PRD
        PostgresPrdContainerName = _appSettings.DatabaseContainer.Prd.ContainerName;
        PostgresPrdAdminUser = _appSettings.DatabaseContainer.Prd.AdminUser;
        PostgresPrdAdminPassword = _appSettings.DatabaseContainer.Prd.AdminPassword;

        // Deployment
        DeploymentBasePath = _appSettings.Deployment.BasePath;
        DeploymentNetworkName = _appSettings.Deployment.NetworkName;
        DeploymentReverseProxyHostStaging = _appSettings.Deployment.ReverseProxyHostStaging;
        DeploymentReverseProxyHostProduction = _appSettings.Deployment.ReverseProxyHostProduction;

        HasUnsavedChanges = false;
    }

    [RelayCommand]
    private Task LoadSettingsAsync()
    {
        LoadFromAppSettings();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        // Update AppSettings object
        _appSettings.Git.Provider = GitProvider;
        _appSettings.Git.BaseUrl = GitBaseUrl;
        _appSettings.Git.ApiToken = GitApiToken;
        _appSettings.Git.DefaultNamespace = GitDefaultNamespace;

        _appSettings.ContainerRegistry.Url = RegistryUrl;
        _appSettings.ContainerRegistry.Username = RegistryUsername;
        _appSettings.ContainerRegistry.Password = RegistryPassword;
        _appSettings.ContainerRegistry.RetentionPolicy.Enabled = RetentionPolicyEnabled;
        _appSettings.ContainerRegistry.RetentionPolicy.RetainCount = RetentionPolicyRetainCount;
        _appSettings.ContainerRegistry.RetentionPolicy.RepositoryPattern = RetentionPolicyRepositoryPattern;
        _appSettings.ContainerRegistry.RetentionPolicy.TagPattern = RetentionPolicyTagPattern;
        _appSettings.ContainerRegistry.RetentionPolicy.IncludeUntagged = RetentionPolicyIncludeUntagged;
        _appSettings.ContainerRegistry.ProjectAdminUsers = ProjectAdminUsers
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        _appSettings.Jenkins.BaseUrl = JenkinsBaseUrl;
        _appSettings.Jenkins.Username = JenkinsUsername;
        _appSettings.Jenkins.ApiToken = JenkinsApiToken;
        _appSettings.Jenkins.DefaultFolder = JenkinsDefaultFolder;
        _appSettings.Jenkins.MaxBuildsToKeep = JenkinsMaxBuildsToKeep;
        _appSettings.Jenkins.Environments = JenkinsEnvironments
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        _appSettings.Jenkins.CredentialsId = JenkinsCredentialsId;

        _appSettings.Servers.Stg.Host = ServerStgHost;
        _appSettings.Servers.Stg.Port = ServerStgPort;
        _appSettings.Servers.Stg.Username = ServerStgUsername;
        _appSettings.Servers.Stg.Password = ServerStgPassword;

        _appSettings.Servers.Prd.Host = ServerPrdHost;
        _appSettings.Servers.Prd.Port = ServerPrdPort;
        _appSettings.Servers.Prd.Username = ServerPrdUsername;
        _appSettings.Servers.Prd.Password = ServerPrdPassword;

        _appSettings.DatabaseContainer.Stg.ContainerName = PostgresStgContainerName;
        _appSettings.DatabaseContainer.Stg.AdminUser = PostgresStgAdminUser;
        _appSettings.DatabaseContainer.Stg.AdminPassword = PostgresStgAdminPassword;

        _appSettings.DatabaseContainer.Prd.ContainerName = PostgresPrdContainerName;
        _appSettings.DatabaseContainer.Prd.AdminUser = PostgresPrdAdminUser;
        _appSettings.DatabaseContainer.Prd.AdminPassword = PostgresPrdAdminPassword;

        _appSettings.Deployment.BasePath = DeploymentBasePath;
        _appSettings.Deployment.NetworkName = DeploymentNetworkName;
        _appSettings.Deployment.ReverseProxyHostStaging = DeploymentReverseProxyHostStaging;
        _appSettings.Deployment.ReverseProxyHostProduction = DeploymentReverseProxyHostProduction;

        // Save to appsettings.local.json
        var settings = new
        {
            Git = new
            {
                _appSettings.Git.Provider,
                _appSettings.Git.BaseUrl,
                _appSettings.Git.ApiToken,
                _appSettings.Git.DefaultNamespace
            },
            ContainerRegistry = new
            {
                _appSettings.ContainerRegistry.Provider,
                _appSettings.ContainerRegistry.Url,
                _appSettings.ContainerRegistry.Username,
                _appSettings.ContainerRegistry.Password,
                RetentionPolicy = new
                {
                    _appSettings.ContainerRegistry.RetentionPolicy.Enabled,
                    _appSettings.ContainerRegistry.RetentionPolicy.RetainCount,
                    _appSettings.ContainerRegistry.RetentionPolicy.RepositoryPattern,
                    _appSettings.ContainerRegistry.RetentionPolicy.TagPattern,
                    _appSettings.ContainerRegistry.RetentionPolicy.IncludeUntagged
                },
                _appSettings.ContainerRegistry.ProjectAdminUsers
            },
            Jenkins = new
            {
                _appSettings.Jenkins.BaseUrl,
                _appSettings.Jenkins.Username,
                _appSettings.Jenkins.ApiToken,
                _appSettings.Jenkins.DefaultFolder,
                _appSettings.Jenkins.MaxBuildsToKeep,
                _appSettings.Jenkins.Environments,
                _appSettings.Jenkins.CredentialsId
            },
            Servers = new
            {
                Stg = new
                {
                    _appSettings.Servers.Stg.Host,
                    _appSettings.Servers.Stg.Port,
                    _appSettings.Servers.Stg.Username,
                    _appSettings.Servers.Stg.Password
                },
                Prd = new
                {
                    _appSettings.Servers.Prd.Host,
                    _appSettings.Servers.Prd.Port,
                    _appSettings.Servers.Prd.Username,
                    _appSettings.Servers.Prd.Password
                }
            },
            PostgresContainer = new
            {
                Stg = new
                {
                    _appSettings.DatabaseContainer.Stg.ContainerName,
                    _appSettings.DatabaseContainer.Stg.AdminUser,
                    _appSettings.DatabaseContainer.Stg.AdminPassword
                },
                Prd = new
                {
                    _appSettings.DatabaseContainer.Prd.ContainerName,
                    _appSettings.DatabaseContainer.Prd.AdminUser,
                    _appSettings.DatabaseContainer.Prd.AdminPassword
                }
            },
            Deployment = new
            {
                _appSettings.Deployment.BasePath,
                _appSettings.Deployment.NetworkName,
                _appSettings.Deployment.ReverseProxyHostStaging,
                _appSettings.Deployment.ReverseProxyHostProduction
            }
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
        await File.WriteAllTextAsync(LocalSettingsPath, json);

        HasUnsavedChanges = false;
    }
}
