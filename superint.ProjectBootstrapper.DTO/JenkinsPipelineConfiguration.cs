using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.DTO;

public sealed class JenkinsPipelineConfiguration
{
    public required string PipelineName { get; init; }
    public string? Folder { get; init; }
    public required PipelineStackType PipelineStackType { get; init; }
    public ProgrammingLanguage ProgrammingLanguage { get; init; } = ProgrammingLanguage.NodeJS;
    public required string ApplicationName { get; init; }
    public required string DeployFolderName { get; init; }
    public string? DotNetProjectName { get; init; }
    public required string GitUrlId { get; init; }
    public required string GitCredentialsId { get; init; }
    public required string ContainerRegistryPathStg { get; init; }
    public required string ContainerRegistryPathPrd { get; init; }
    public string? ReverseProxyPathPrefixStg { get; init; }
    public string? ReverseProxyPathPrefixPrd { get; init; }
}
