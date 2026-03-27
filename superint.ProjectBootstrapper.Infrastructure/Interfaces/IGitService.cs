using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface IGitService
    {
        Task<OperationResult> CreateRepositoryAsync(string repositoryName, string description, string? namespacePath = null, bool autoInit = false, CancellationToken cancellationToken = default);
        Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
    }
}