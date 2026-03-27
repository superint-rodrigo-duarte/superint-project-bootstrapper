using System.Text.Json.Serialization;

namespace superint.ProjectBootstrapper.DTO
{
    public sealed class ContainerRegistryConfigurationJson
    {
        [JsonPropertyName("createProject")]
        public bool CreateProject { get; set; } = true;
        [JsonPropertyName("projectName")]
        public string? ProjectName { get; set; }
        [JsonPropertyName("pathBackendStg")]
        public string? PathBackendStg { get; set; }
        [JsonPropertyName("pathBackendPrd")]
        public string? PathBackendPrd { get; set; }
        [JsonPropertyName("pathFrontendStg")]
        public string? PathFrontendStg { get; set; }
        [JsonPropertyName("pathFrontendPrd")]
        public string? PathFrontendPrd { get; set; }
    }
}