namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class DatabaseContainerSettings
    {
        public DatabaseContainerEnvironmentSettings Stg { get; set; } = new();
        public DatabaseContainerEnvironmentSettings Prd { get; set; } = new();
    }
}