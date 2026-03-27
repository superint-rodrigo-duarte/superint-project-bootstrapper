namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class DeploymentSettings
    {
        public string BasePath { get; set; } = "";
        public string NetworkName { get; set; } = "superint-network";
        public string ReverseProxyHostStaging { get; set; } = "";
        public string ReverseProxyHostProduction { get; set; } = "";
    }
}