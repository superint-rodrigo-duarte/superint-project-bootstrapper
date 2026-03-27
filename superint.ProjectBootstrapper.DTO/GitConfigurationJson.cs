using System.Text.Json.Serialization;

namespace superint.ProjectBootstrapper.DTO
{
    public sealed class GitConfigurationJson
    {
        [JsonPropertyName("createRepository")]
        public bool CreateRepository { get; set; } = true;
        [JsonPropertyName("initializeWithReadme")]
        public bool InitializeWithReadme { get; set; } = false;
        [JsonPropertyName("namespace")]
        public string? Namespace { get; set; }
        [JsonPropertyName("repositoryName")]
        public string? RepositoryName { get; set; }
        [JsonPropertyName("repoitoryUrl")]
        public string? RepositoryUrl { get; set; }
        [JsonPropertyName("backendRepositoryUrl")]
        public string? BackendRepositoryUrl { get; set; }
        [JsonPropertyName("frontendRepositoryUrl")]
        public string? FrontendRepositoryUrl { get; set; }
    }
}