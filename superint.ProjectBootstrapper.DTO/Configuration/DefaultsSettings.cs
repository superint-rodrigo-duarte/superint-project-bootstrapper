namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class DefaultsSettings
    {
        public string DockerRegistry { get; set; } = string.Empty;
        public string NetworkName { get; set; } = "app-network";
    }
}