using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface IJenkinsfileService
    {
        string GenerateJenkinsfile(ProjectConfiguration dtoProjectConfiguration, StackType stackType);
        Task<OperationResult> SaveJenkinsfileAsync(ProjectConfiguration dtoProjectConfiguration, StackType stackType, string outputPath, CancellationToken cancellationToken = default);
    }
}