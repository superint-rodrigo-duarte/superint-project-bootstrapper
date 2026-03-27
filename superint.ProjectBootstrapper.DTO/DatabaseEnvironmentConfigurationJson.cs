using System.Text.Json.Serialization;

namespace superint.ProjectBootstrapper.DTO
{
    public sealed class DatabaseEnvironmentConfigurationJson
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }
}