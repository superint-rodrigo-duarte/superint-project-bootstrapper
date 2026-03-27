using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface IDatabaseService
    {
        string Environment { get; }
        Task<OperationResult> CreateUserAndDatabaseAsync(string databaseName, string username, string password, CancellationToken cancellationToken = default);
        Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
    }
}