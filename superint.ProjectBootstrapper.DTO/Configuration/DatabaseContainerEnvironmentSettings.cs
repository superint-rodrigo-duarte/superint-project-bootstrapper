namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class DatabaseContainerEnvironmentSettings
    {
        public string ContainerName { get; set; } = string.Empty;
        public string AdminUser { get; set; } = "postgres";
        public string AdminPassword { get; set; } = string.Empty;
    }
}