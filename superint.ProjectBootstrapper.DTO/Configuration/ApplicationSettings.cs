namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class ApplicationSettings
    {
        public GitSettings Git { get; set; } = new();
        public ContainerRegistrySettings ContainerRegistry { get; set; } = new();
        public JenkinsSettings Jenkins { get; set; } = new();
        public ServersSettings Servers { get; set; } = new();
        public DatabaseContainerSettings DatabaseContainer { get; set; } = new();
        public DeploymentSettings Deployment { get; set; } = new();
    }
}