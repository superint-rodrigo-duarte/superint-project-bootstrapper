using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface IJenkinsService
    {
        Task<OperationResult> CreateFolderAsync(string folderName, string? parentFolder = null, CancellationToken cancellationToken = default);
        Task<OperationResult> CreatePipelineAsync(JenkinsPipelineConfiguration dtoJenkinsPipelineConfig, CancellationToken cancellationToken = default);
        Task<OperationResult> CreateSecretTextCredentialAsync(string credentialId, string credentialName, string secretValue, CancellationToken cancellationToken = default);
        Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
    }
}