using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface IDockerComposeService
    {
        Task<OperationResult> GenerateDockerComposeAsync(ProjectConfiguration dtoProjectConfiguration, string environment, string outputPath, CancellationToken cancellationToken = default);
    }
}