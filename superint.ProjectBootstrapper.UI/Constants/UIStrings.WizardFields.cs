namespace superint.ProjectBootstrapper.UI.Constants;

public static partial class UIStrings
{
    public static class WizardFields
    {
        // Basic Info
        public const string BasicInformation = "Basic Information";

        // Git
        public const string GitRepository = "Git Repository";
        public const string RepositoryUrl = "Repository URL";
        public const string DefaultBranch = "Default Branch";
        public const string GitDisabledMessage = "Git integration is disabled. Enable it to configure repository settings.";

        // Container Registry
        public const string RegistryUrl = "Registry URL";
        public const string RegistryDisabledMessage = "Container Registry is disabled. Enable it to configure image settings.";

        // Jenkins
        public const string JenkinsUrl = "Jenkins URL";
        public const string JobName = "Job Name";
        public const string JenkinsDisabledMessage = "Jenkins integration is disabled. Enable it to configure CI/CD settings.";

        // Database
        public const string ConnectionString = "Connection String";
        public const string DatabaseDisabledMessage = "Database configuration is disabled. Enable it to configure database settings.";

        // Docker
        public const string DockerConfiguration = "Docker Configuration";
        public const string ReverseProxyHost = "ReverseProxy Host";
        public const string InternalPort = "Internal Port";
        public const string ExternalPort = "External Port";
        public const string DockerDisabledMessage = "Docker Compose configuration is disabled. Enable it to configure container settings.";

        // Review
        public const string ConfigurationReview = "Configuration Review";
        public const string ReviewWarning = "Please review all settings carefully before executing the bootstrap process.";
    }
}
