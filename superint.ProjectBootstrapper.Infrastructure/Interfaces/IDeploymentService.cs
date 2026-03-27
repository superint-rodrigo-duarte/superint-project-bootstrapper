using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface IDeploymentService
    {
        string Environment { get; }
        Task<OperationResult> DeployDockerComposeAsync(ProjectConfiguration dtoProjectConfiguration, CancellationToken cancellationToken = default);
        Task<OperationResult> StartServicesAsync(ProjectConfiguration dtoProjectConfiguration, CancellationToken cancellationToken = default);
        Task<OperationResult> StopServicesAsync(ProjectConfiguration dtoProjectConfiguration, CancellationToken cancellationToken = default);
    }
}