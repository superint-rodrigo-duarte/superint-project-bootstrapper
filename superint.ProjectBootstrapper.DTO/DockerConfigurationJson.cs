using System.Text.Json.Serialization;
namespace superint.ProjectBootstrapper.DTO
{
    public sealed class DockerConfigurationJson
    {
        [JsonPropertyName("generateCompose")]
        public bool GenerateCompose { get; set; } = true;
        [JsonPropertyName("deployCompose")]
        public bool DeployCompose { get; set; } = true;
        [JsonPropertyName("deployFolderName")]
        public string? DeployFolderName { get; set; }
        [JsonPropertyName("containerNameFrontend")]
        public string? ContainerNameFrontend { get; set; }
        [JsonPropertyName("containerNameBackend")]
        public string? ContainerNameBackend { get; set; }
        [JsonPropertyName("stg")]
        public DockerEnvironmentConfigurationJson? Stg { get; set; }
        [JsonPropertyName("prd")]
        public DockerEnvironmentConfigurationJson? Prd { get; set; }
    }
}