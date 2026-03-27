using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface IContainerRegistryService
    {
        Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
        Task<OperationResult> CreateProjectAsync(string projectName, int retentionDays = 30, CancellationToken cancellationToken = default);
    }
}