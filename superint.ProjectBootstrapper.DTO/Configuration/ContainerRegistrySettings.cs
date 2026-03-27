namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class ContainerRegistrySettings
    {
        public string Provider { get; set; } = "harbor";
        public string Url { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public ContainerRegistryRetentionPolicySettings RetentionPolicy { get; set; } = new();
        public List<string> ProjectAdminUsers { get; set; } = [];
    }
}