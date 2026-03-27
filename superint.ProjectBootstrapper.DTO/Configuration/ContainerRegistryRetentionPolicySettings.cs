namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class ContainerRegistryRetentionPolicySettings
    {
        public bool Enabled { get; set; } = true;
        public int RetainCount { get; set; } = 5;
        public string RepositoryPattern { get; set; } = "**";
        public string TagPattern { get; set; } = "**";
        public bool IncludeUntagged { get; set; } = true;
    }
}