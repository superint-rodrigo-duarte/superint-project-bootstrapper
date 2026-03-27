using System.Text.Json.Serialization;

namespace superint.ProjectBootstrapper.DTO
{
    public sealed class DatabaseConfigurationJson
    {
        [JsonPropertyName("createUser")]
        public bool CreateUser { get; set; } = true;
        [JsonPropertyName("stg")]
        public DatabaseEnvironmentConfigurationJson? Stg { get; set; }
        [JsonPropertyName("prd")]
        public DatabaseEnvironmentConfigurationJson? Prd { get; set; }
    }
}