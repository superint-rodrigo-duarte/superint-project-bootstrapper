using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.Infrastructure.Interfaces
{
    public interface ISshService
    {
        string Environment { get; }
        Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
        Task<OperationResult> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
        Task<OperationResult> UploadFileAsync(string localContent, string remotePath, CancellationToken cancellationToken = default);
        Task<OperationResult> EnsureDirectoryExistsAsync(string remotePath, CancellationToken cancellationToken = default);
        Task<bool> DirectoryExistsAsync(string remotePath, CancellationToken cancellationToken = default);
    }
}