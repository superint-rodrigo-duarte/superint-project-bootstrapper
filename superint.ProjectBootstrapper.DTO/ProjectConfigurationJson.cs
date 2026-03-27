using superint.ProjectBootstrapper.Shared.Enums;
using System.Text.Json.Serialization;

namespace superint.ProjectBootstrapper.DTO;

public sealed class ProjectConfigurationJson
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProjectType Type { get; set; } = ProjectType.Backend;
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("frontendLanguage")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProgrammingLanguage FrontendProgrammingLanguage { get; set; } = ProgrammingLanguage.NodeJS;
    [JsonPropertyName("backendLanguage")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProgrammingLanguage BackendProgrammingLanguage { get; set; } = ProgrammingLanguage.NodeJS;
    [JsonPropertyName("dotNetProjectName")]
    public string? DotNetProjectName { get; set; }
    [JsonPropertyName("git")]
    public GitConfigurationJson? Git { get; set; }
    [JsonPropertyName("containerRegistry")]
    public ContainerRegistryConfigurationJson? ContainerRegistry { get; set; }
    [JsonPropertyName("jenkins")]
    public JenkinsConfigurationJson? Jenkins { get; set; }
    [JsonPropertyName("database")]
    public DatabaseConfigurationJson? Database { get; set; }
    [JsonPropertyName("docker")]
    public DockerConfigurationJson? Docker { get; set; }
}