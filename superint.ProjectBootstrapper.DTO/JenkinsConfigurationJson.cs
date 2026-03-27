using System.Text.Json.Serialization;

namespace superint.ProjectBootstrapper.DTO
{
    public sealed class JenkinsConfigurationJson
    {
        [JsonPropertyName("createPipeline")]
        public bool CreatePipeline { get; set; } = true;
        [JsonPropertyName("folder")]
        public string? Folder { get; set; }
        [JsonPropertyName("projectFolderName")]
        public string? ProjectFolderName { get; set; }
        [JsonPropertyName("backendPipelineName")]
        public string? BackendPipelineName { get; set; }
        [JsonPropertyName("frontendPipelineName")]
        public string? FrontendPipelineName { get; set; }
        [JsonPropertyName("gitCredentialsId")]
        public string? GitCredentialsId { get; set; }
        [JsonPropertyName("createGitCredentialBackend")]
        public bool? CreateGitCredentialBackend { get; set; }
        [JsonPropertyName("createGitCredentialFrontend")]
        public bool? CreateGitCredentialFrontend { get; set; }
        [JsonPropertyName("existingGitUrlIdBackend")]
        public string? ExistingGitUrlIdBackend { get; set; }
        [JsonPropertyName("existingGitUrlIdFrontend")]
        public string? ExistingGitUrlIdFrontend { get; set; }
    }
}