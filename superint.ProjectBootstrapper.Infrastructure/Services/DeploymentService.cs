using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class DeploymentService(ISshService sshService, IDockerComposeService dockerComposeService, DeploymentSettings deploymentSettings, string environment) : IDeploymentService
    {
        public string Environment { get; } = environment;

        private string GetProjectPath(ProjectConfiguration dtoProjectConfiguration)
        {
            return $"{deploymentSettings.BasePath}/{dtoProjectConfiguration.DeployFolderName}";
        }

        public async Task<OperationResult> DeployDockerComposeAsync(ProjectConfiguration dtoProjectConfiguration, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var projectPath = GetProjectPath(dtoProjectConfiguration);

                var ensureDirResult = await sshService.EnsureDirectoryExistsAsync(projectPath, cancellationToken);
                if (!ensureDirResult.Success)
                    return OperationResult.Fail($"Falha ao criar diretório: {ensureDirResult.Message}");

                var dockerComposeContent = ((DockerComposeService)dockerComposeService).GenerateDockerComposeContent(dtoProjectConfiguration, Environment);

                var remotePath = $"{projectPath}/docker-compose.yml";

                var uploadResult = await sshService.UploadFileAsync(dockerComposeContent, remotePath, cancellationToken);
                if (!uploadResult.Success)
                    return OperationResult.Fail($"Falha ao enviar docker-compose: {uploadResult.Message}");

                return OperationResult.Ok($"Docker Compose implantado ({Environment})", $"Caminho: {remotePath}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao implantar docker-compose ({Environment})", ex);
            }
        }

        public async Task<OperationResult> StartServicesAsync(ProjectConfiguration dtoProjectConfiguration, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var projectPath = GetProjectPath(dtoProjectConfiguration);

                var command = $"cd {projectPath} && docker compose pull && docker compose up -d";

                var result = await sshService.ExecuteCommandAsync(command, cancellationToken);
                if (!result.Success)
                    return OperationResult.Fail($"Falha ao iniciar serviços: {result.Message}");

                return OperationResult.Ok($"Serviços iniciados ({Environment})", $"Projeto: {dtoProjectConfiguration.ProjectSlug}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao iniciar serviços ({Environment})", ex);
            }
        }

        public async Task<OperationResult> StopServicesAsync(ProjectConfiguration dtoProjectConfiguration, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var projectPath = GetProjectPath(dtoProjectConfiguration);

                var command = $"cd {projectPath} && docker compose down";

                var result = await sshService.ExecuteCommandAsync(command, cancellationToken);
                if (!result.Success)
                    return OperationResult.Fail($"Falha ao parar serviços: {result.Message}");

                return OperationResult.Ok($"Serviços parados ({Environment})", $"Projeto: {dtoProjectConfiguration.ProjectSlug}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao parar serviços ({Environment})", ex);
            }
        }
    }
}