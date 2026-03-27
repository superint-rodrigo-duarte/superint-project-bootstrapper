namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class GitSettings
    {
        public string Provider { get; set; } = "github";
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
        public string DefaultNamespace { get; set; } = string.Empty;
    }
}