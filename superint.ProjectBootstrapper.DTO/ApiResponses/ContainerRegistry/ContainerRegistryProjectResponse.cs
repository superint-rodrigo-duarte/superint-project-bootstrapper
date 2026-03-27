using System.Text.Json.Serialization;

namespace superint.ProjectBootstrapper.DTO.ApiResponses.ContainerRegistry
{
    public sealed class ContainerRegistryProjectResponse
    {
        [JsonPropertyName("project_id")]
        public int ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}