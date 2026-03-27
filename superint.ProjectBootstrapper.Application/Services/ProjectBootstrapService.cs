using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.Application.Services
{
    public sealed class ProjectBootstrapService(ApplicationSettings appSettings,
                                                IGitService gitService,
                                                IContainerRegistryService containerRegistryService,
                                                IJenkinsService jenkinsService,
                                                IDatabaseService databaseHmlService,
                                                IDatabaseService databasePrdService,
                                                IDockerComposeService dockerComposeService,
                                                ISshService sshHmlService,
                                                ISshService sshPrdService,
                                                IDeploymentService deploymentHmlService,
                                                IDeploymentService deploymentPrdService)
    {
        private static JenkinsPipelineConfiguration BuildJenkinsPipelineConfiguration(ProjectConfiguration dtoProjectConfiguration,
                                                                               PipelineStackType pipelineStackType,
                                                                               string repositoryName)
        {
            var jenkinsProjectFolderName = dtoProjectConfiguration.JenkinsProjectFolderName;
            var jenkinsProjectFolderFullPath = string.IsNullOrEmpty(dtoProjectConfiguration.JenkinsFolder) ? jenkinsProjectFolderName : $"{dtoProjectConfiguration.JenkinsFolder}/{jenkinsProjectFolderName}";

            var jenkinsPipelineName = pipelineStackType == PipelineStackType.Backend ? dtoProjectConfiguration.JenkinsBackendPipelineName : dtoProjectConfiguration.JenkinsFrontendPipelineName;
            var codeLanguage = pipelineStackType == PipelineStackType.Backend ? dtoProjectConfiguration.BackendProgrammingLanguage : dtoProjectConfiguration.FrontendProgrammingLanguage;

            return new JenkinsPipelineConfiguration
            {
                PipelineName = jenkinsPipelineName,
                Folder = jenkinsProjectFolderFullPath,
                PipelineStackType = pipelineStackType,
                ProgrammingLanguage = codeLanguage,
                ApplicationName = dtoProjectConfiguration.ProjectSlug,
                DeployFolderName = dtoProjectConfiguration.DeployFolderName,
                DotNetProjectName = codeLanguage == ProgrammingLanguage.CSharp ? dtoProjectConfiguration.DotNetProjectName : null,
                GitUrlId = dtoProjectConfiguration.GetEffectiveGitUrlId(pipelineStackType, repositoryName),
                GitCredentialsId = dtoProjectConfiguration.JenkinsGitCredentialsId,
                ContainerRegistryPathStg = pipelineStackType == PipelineStackType.Backend ? dtoProjectConfiguration.ContainerRegistryPathBackendStg : dtoProjectConfiguration.ContainerRegistryPathFrontendStg,
                ContainerRegistryPathPrd = pipelineStackType == PipelineStackType.Backend ? dtoProjectConfiguration.ContainerRegistryPathBackendPrd : dtoProjectConfiguration.ContainerRegistryPathFrontendPrd,
                ReverseProxyPathPrefixStg = pipelineStackType == PipelineStackType.Frontend ? dtoProjectConfiguration.ReverseProxyPathPrefixStg : null,
                ReverseProxyPathPrefixPrd = pipelineStackType == PipelineStackType.Frontend ? dtoProjectConfiguration.ReverseProxyPathPrefixPrd : null
            };
        }

        private static string GetGitRepositoryName(string defaultRepositoryName, string? gitUrl)
        {
            if (!string.IsNullOrEmpty(gitUrl))
                return ExtractGitRepositoryNameFromUrl(gitUrl);

            return defaultRepositoryName;
        }

        private static string ExtractGitRepositoryNameFromUrl(string gitUrl)
        {
            if (string.IsNullOrEmpty(gitUrl))
                return string.Empty;

            var url = gitUrl.TrimEnd('/');

            if (url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                url = url[..^4];

            var lastSlashIndex = url.LastIndexOf('/');

            if (lastSlashIndex >= 0 && lastSlashIndex < url.Length - 1)
                return url[(lastSlashIndex + 1)..];

            return url;
        }

        private string GetGitRepositoryUrl(ProjectConfiguration dtoProjectConfiguration, string repositoryName, string? providedUrl)
        {
            if (!string.IsNullOrEmpty(providedUrl))
                return providedUrl;

            var gitNamespace = dtoProjectConfiguration.GitNamespace ?? appSettings.Git.DefaultNamespace;
            var provider = appSettings.Git.Provider?.ToLowerInvariant();

            return provider switch
            {
                "github" => $"https://github.com/{gitNamespace}/{repositoryName}.git",
                _ => $"{appSettings.Git.BaseUrl.TrimEnd('/')}/{gitNamespace}/{repositoryName}.git"
            };
        }

        private static Task<OperationResult> CopyDockerComposeLocalAsync(string sourcePath, string destinationPath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    return Task.FromResult(OperationResult.Fail($"Arquivo origem não encontrado: {sourcePath}"));

                var destinationDirectoryName = Path.GetDirectoryName(destinationPath);

                if (!string.IsNullOrEmpty(destinationDirectoryName))
                    Directory.CreateDirectory(destinationDirectoryName);

                File.Copy(sourcePath, destinationPath, overwrite: true);

                return Task.FromResult(OperationResult.Ok("Docker Compose copiado com sucesso", $"Destino: {destinationPath}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(OperationResult.Fail($"Falha ao copiar Docker Compose: {ex.Message}", ex));
            }
        }

        public async Task<Dictionary<string, OperationResult>> TestConnectionsAsync(CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, OperationResult>
            {
                ["Git"] = await gitService.ValidateConnectionAsync(cancellationToken),
                ["Container Registry"] = await containerRegistryService.ValidateConnectionAsync(cancellationToken),
                ["Jenkins"] = await jenkinsService.ValidateConnectionAsync(cancellationToken),
                ["SSH (STG)"] = await sshHmlService.ValidateConnectionAsync(cancellationToken),
                ["SSH (PRD)"] = await sshPrdService.ValidateConnectionAsync(cancellationToken),
                ["Database (STG)"] = await databaseHmlService.ValidateConnectionAsync(cancellationToken),
                ["Database (PRD)"] = await databasePrdService.ValidateConnectionAsync(cancellationToken)
            };

            return results;
        }

        public async Task<List<(string Name, OperationResult Result)>> ExecuteBootstrapAsync(ProjectConfiguration dtoProjectConfiguration,
                                                                                             Action<string, OperationResult>? onTaskCompleted = null,
                                                                                             CancellationToken cancellationToken = default)
        {
            var results = new List<(string Name, OperationResult Result)>();
            var taskList = BuildTaskList(dtoProjectConfiguration);

            foreach (var (name, action, enabled) in taskList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                OperationResult result;

                if (enabled)
                {
                    result = await action();
                }
                else
                {
                    result = OperationResult.Skip("Desabilitado pelo usuário");
                }

                results.Add((name, result));
                onTaskCompleted?.Invoke(name, result);
            }

            return results;
        }

        public List<(string Name, Func<Task<OperationResult>> Action, bool Enabled)> BuildTaskList(ProjectConfiguration dtoProjectConfiguration)
        {
            var taskList = new List<(string Name, Func<Task<OperationResult>> Action, bool Enabled)>();

            // Git repositories
            if (dtoProjectConfiguration.CreateGitRepository)
            {
                if (dtoProjectConfiguration.ProjectType == ProjectType.Fullstack)
                {
                    taskList.Add(("Criar Repositório Backend", () => gitService.CreateRepositoryAsync(dtoProjectConfiguration.BackendRepositoryName,
                                                                                                      dtoProjectConfiguration.Description,
                                                                                                      dtoProjectConfiguration.GitNamespace,
                                                                                                      dtoProjectConfiguration.InitializeGitRepositoryWithReadme),
                                                                                                      true));

                    taskList.Add(("Criar Repositório Frontend", () => gitService.CreateRepositoryAsync(dtoProjectConfiguration.FrontendRepositoryName,
                                                                                                       dtoProjectConfiguration.Description,
                                                                                                       dtoProjectConfiguration.GitNamespace,
                                                                                                       dtoProjectConfiguration.InitializeGitRepositoryWithReadme),
                                                                                                       true));
                }
                else
                {
                    taskList.Add(("Criar Repositório Git", () => gitService.CreateRepositoryAsync(dtoProjectConfiguration.GitRepositoryName,
                                                                                                  dtoProjectConfiguration.Description,
                                                                                                  dtoProjectConfiguration.GitNamespace,
                                                                                                  dtoProjectConfiguration.InitializeGitRepositoryWithReadme),
                                                                                                  true));
                }
            }

            // Container Registry project
            if (dtoProjectConfiguration.CreateContainerRegistryProject)
            {
                taskList.Add(("Criar Projeto Container Registry", () => containerRegistryService.CreateProjectAsync(dtoProjectConfiguration.ContainerRegistryProjectName), true));
            }

            // Jenkins pipelines
            if (dtoProjectConfiguration.CreateJenkinsPipeline)
            {
                var jenkinsProjectFolderName = dtoProjectConfiguration.JenkinsProjectFolderName;
                var jenkinsProjectFolderFullPath = string.IsNullOrEmpty(dtoProjectConfiguration.JenkinsFolder) ? jenkinsProjectFolderName : $"{dtoProjectConfiguration.JenkinsFolder}/{jenkinsProjectFolderName}";

                taskList.Add(("Criar Pasta Jenkins", () => jenkinsService.CreateFolderAsync(jenkinsProjectFolderName, dtoProjectConfiguration.JenkinsFolder), true));

                // Criar credenciais de URL Git antes dos pipelines (per-stack)
                if (dtoProjectConfiguration.ProjectType == ProjectType.Fullstack)
                {
                    var backendRepositoryName = GetGitRepositoryName(dtoProjectConfiguration.BackendRepositoryName, dtoProjectConfiguration.BackendGitUrl);
                    var frontendRepositoryName = GetGitRepositoryName(dtoProjectConfiguration.FrontendRepositoryName, dtoProjectConfiguration.FrontendGitUrl);

                    var backendGitUrl = GetGitRepositoryUrl(dtoProjectConfiguration, backendRepositoryName, dtoProjectConfiguration.BackendGitUrl);
                    var frontendGitUrl = GetGitRepositoryUrl(dtoProjectConfiguration, frontendRepositoryName, dtoProjectConfiguration.FrontendGitUrl);

                    taskList.Add(("Criar Credencial URL Git (Backend)", () => jenkinsService.CreateSecretTextCredentialAsync(ProjectConfiguration.GetJenkinsGitUrlCredentialId(backendRepositoryName),
                                                                                                                             ProjectConfiguration.GetJenkinsGitUrlCredentialName(backendRepositoryName),
                                                                                                                             backendGitUrl),
                                                                                                                             dtoProjectConfiguration.CreateJenkinsGitCredentialBackend));

                    taskList.Add(("Criar Credencial URL Git (Frontend)", () => jenkinsService.CreateSecretTextCredentialAsync(ProjectConfiguration.GetJenkinsGitUrlCredentialId(frontendRepositoryName),
                                                                                                                              ProjectConfiguration.GetJenkinsGitUrlCredentialName(frontendRepositoryName),
                                                                                                                              frontendGitUrl),
                                                                                                                              dtoProjectConfiguration.CreateJenkinsGitCredentialFrontend));

                    var projectBackendConfiguration = BuildJenkinsPipelineConfiguration(dtoProjectConfiguration, PipelineStackType.Backend, backendRepositoryName);
                    var projectFrontendConfiguration = BuildJenkinsPipelineConfiguration(dtoProjectConfiguration, PipelineStackType.Frontend, frontendRepositoryName);

                    taskList.Add(("Criar Pipeline Backend", () => jenkinsService.CreatePipelineAsync(projectBackendConfiguration), true));
                    taskList.Add(("Criar Pipeline Frontend", () => jenkinsService.CreatePipelineAsync(projectFrontendConfiguration), true));
                }
                else if (dtoProjectConfiguration.ProjectType == ProjectType.Backend)
                {
                    var gitRepositoryName = GetGitRepositoryName(dtoProjectConfiguration.GitRepositoryName, dtoProjectConfiguration.GitRepositoryUrl);
                    var gitUrl = GetGitRepositoryUrl(dtoProjectConfiguration, gitRepositoryName, dtoProjectConfiguration.GitRepositoryUrl);

                    taskList.Add(("Criar Credencial URL Git", () => jenkinsService.CreateSecretTextCredentialAsync(ProjectConfiguration.GetJenkinsGitUrlCredentialId(gitRepositoryName),
                                                                                                                   ProjectConfiguration.GetJenkinsGitUrlCredentialName(gitRepositoryName),
                                                                                                                   gitUrl),
                                                                                                                   dtoProjectConfiguration.CreateJenkinsGitCredentialBackend));

                    var jenkinsPipelineConfiguration = BuildJenkinsPipelineConfiguration(dtoProjectConfiguration, PipelineStackType.Backend, gitRepositoryName);
                    taskList.Add(("Criar Pipeline", () => jenkinsService.CreatePipelineAsync(jenkinsPipelineConfiguration), true));
                }
                else
                {
                    var gitRepositoryName = GetGitRepositoryName(dtoProjectConfiguration.GitRepositoryName, dtoProjectConfiguration.GitRepositoryUrl);
                    var gitUrl = GetGitRepositoryUrl(dtoProjectConfiguration, gitRepositoryName, dtoProjectConfiguration.GitRepositoryUrl);

                    taskList.Add(("Criar Credencial URL Git", () => jenkinsService.CreateSecretTextCredentialAsync(ProjectConfiguration.GetJenkinsGitUrlCredentialId(gitRepositoryName),
                                                                                                                   ProjectConfiguration.GetJenkinsGitUrlCredentialName(gitRepositoryName),
                                                                                                                   gitUrl),
                                                                                                                   dtoProjectConfiguration.CreateJenkinsGitCredentialFrontend));

                    var jenkinsPipelineConfiguration = BuildJenkinsPipelineConfiguration(dtoProjectConfiguration, PipelineStackType.Frontend, gitRepositoryName);
                    taskList.Add(("Criar Pipeline", () => jenkinsService.CreatePipelineAsync(jenkinsPipelineConfiguration), true));
                }
            }

            // Database user (apenas usuário, banco criado via migração)
            if (dtoProjectConfiguration.CreateDatabaseUser)
            {
                // O parâmetro databaseName é ignorado pelo serviço, mas mantido por compatibilidade com a interface
                if (dtoProjectConfiguration.CreateDatabaseUserStg && !string.IsNullOrEmpty(dtoProjectConfiguration.DatabasePasswordStg))
                {
                    taskList.Add(("Criar Usuário PostgreSQL (STG)", () => databaseHmlService.CreateUserAndDatabaseAsync(string.Empty,
                                                                                                                        dtoProjectConfiguration.DatabaseUserStg,
                                                                                                                        dtoProjectConfiguration.DatabasePasswordStg),
                                                                                                                        true));
                }

                if (dtoProjectConfiguration.CreateDatabaseUserPrd && !string.IsNullOrEmpty(dtoProjectConfiguration.DatabasePasswordPrd))
                {
                    taskList.Add(("Criar Usuário PostgreSQL (PRD)", () => databasePrdService.CreateUserAndDatabaseAsync(string.Empty,
                                                                                                                        dtoProjectConfiguration.DatabaseUserPrd,
                                                                                                                        dtoProjectConfiguration.DatabasePasswordPrd),
                                                                                                                        true));
                }
            }

            // Docker Compose generation and deployment
            if (dtoProjectConfiguration.GenerateDockerCompose)
            {
                var dockerComposePath = Path.Combine(Environment.CurrentDirectory, "output", dtoProjectConfiguration.ProjectSlug);

                taskList.Add(("Gerar Docker Compose (STG)", () => dockerComposeService.GenerateDockerComposeAsync(dtoProjectConfiguration,
                                                                                                                  "stg",
                                                                                                                  Path.Combine(dockerComposePath, "stg")),
                                                                                                                  true));

                taskList.Add(("Gerar Docker Compose (PRD)", () => dockerComposeService.GenerateDockerComposeAsync(dtoProjectConfiguration,
                                                                                                                  "prd",
                                                                                                                  Path.Combine(dockerComposePath, "prd")),
                                                                                                                  true));
            }

            // Deploy Docker Compose to servers (apenas se também estiver gerando)
            if (dtoProjectConfiguration.GenerateDockerCompose && dtoProjectConfiguration.DeployDockerCompose)
            {
                if (dtoProjectConfiguration.DeployDockerComposeStg)
                {
                    taskList.Add(("Deploy Docker Compose (STG)", () => deploymentHmlService.DeployDockerComposeAsync(dtoProjectConfiguration), true));
                }

                if (dtoProjectConfiguration.DeployDockerComposePrd)
                {
                    taskList.Add(("Deploy Docker Compose (PRD)", () => deploymentPrdService.DeployDockerComposeAsync(dtoProjectConfiguration), true));
                }
            }

            // Copiar Docker Compose para espelho local do servidor
            if (dtoProjectConfiguration.GenerateDockerCompose)
            {
                var dockerComposePath = Path.Combine(Environment.CurrentDirectory, "output", dtoProjectConfiguration.ProjectSlug);

                if (dtoProjectConfiguration.CopyDockerComposeLocalStg && !string.IsNullOrEmpty(dtoProjectConfiguration.LocalDeployPathStg))
                {
                    taskList.Add(("Copiar Docker Compose Local (STG)", () => CopyDockerComposeLocalAsync(Path.Combine(dockerComposePath, "stg", "docker-compose.yml"),
                                                                                                         Path.Combine(dtoProjectConfiguration.LocalDeployPathStg, "docker-compose.yml")),
                                                                                                         true));
                }

                if (dtoProjectConfiguration.CopyDockerComposeLocalPrd && !string.IsNullOrEmpty(dtoProjectConfiguration.LocalDeployPathPrd))
                {
                    taskList.Add(("Copiar Docker Compose Local (PRD)", () => CopyDockerComposeLocalAsync(Path.Combine(dockerComposePath, "prd", "docker-compose.yml"),
                                                                                                         Path.Combine(dtoProjectConfiguration.LocalDeployPathPrd, "docker-compose.yml")),
                                                                                                         true));
                }
            }

            return taskList;
        }
    }
}