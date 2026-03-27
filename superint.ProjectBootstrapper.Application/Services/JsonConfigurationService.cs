using superint.ProjectBootstrapper.DTO;
using System.Text.Json;

namespace superint.ProjectBootstrapper.Application.Services
{
    public sealed class JsonConfigurationService
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task<ProjectConfiguration> LoadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Arquivo de configuração não encontrado: {filePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var jsonConfig = JsonSerializer.Deserialize<ProjectConfigurationJson>(jsonContent, jsonSerializerOptions)
                             ?? throw new InvalidOperationException("Falha ao deserializar o arquivo JSON");

            return ConvertToProjectConfiguration(jsonConfig);
        }

        private static ProjectConfiguration ConvertToProjectConfiguration(ProjectConfigurationJson json)
        {
            var config = new ProjectConfiguration
            {
                ProjectName = json.ProjectName,
                ProjectType = json.Type,
                Description = json.Description ?? string.Empty,
                FrontendProgrammingLanguage = json.FrontendProgrammingLanguage,
                BackendProgrammingLanguage = json.BackendProgrammingLanguage,
                DotNetProjectName = json.DotNetProjectName
            };

            if (json.Git != null)
            {
                config.CreateGitRepository = json.Git.CreateRepository;
                config.InitializeGitRepositoryWithReadme = json.Git.InitializeWithReadme;
                config.GitNamespace = json.Git.Namespace;
                config.GitRepositoryName = json.Git.RepositoryName ?? config.GetDefaultGitRepoName();
                config.GitRepositoryUrl = json.Git.RepositoryUrl;
                config.BackendGitUrl = json.Git.BackendRepositoryUrl;
                config.FrontendGitUrl = json.Git.FrontendRepositoryUrl;
            }
            else
            {
                config.CreateGitRepository = false;
                config.GitRepositoryName = config.GetDefaultGitRepoName();
            }

            if (json.ContainerRegistry != null)
            {
                config.CreateContainerRegistryProject = json.ContainerRegistry.CreateProject;
                config.ContainerRegistryProjectName = json.ContainerRegistry.ProjectName ?? config.ProjectSlug;

                config.AutoFillContainerRegistryPaths();

                if (!string.IsNullOrEmpty(json.ContainerRegistry.PathBackendStg))
                    config.ContainerRegistryPathBackendStg = json.ContainerRegistry.PathBackendStg;
                if (!string.IsNullOrEmpty(json.ContainerRegistry.PathBackendPrd))
                    config.ContainerRegistryPathBackendPrd = json.ContainerRegistry.PathBackendPrd;
                if (!string.IsNullOrEmpty(json.ContainerRegistry.PathFrontendStg))
                    config.ContainerRegistryPathFrontendStg = json.ContainerRegistry.PathFrontendStg;
                if (!string.IsNullOrEmpty(json.ContainerRegistry.PathFrontendPrd))
                    config.ContainerRegistryPathFrontendPrd = json.ContainerRegistry.PathFrontendPrd;
            }
            else
            {
                config.CreateContainerRegistryProject = false;
                config.ContainerRegistryProjectName = config.ProjectSlug;
                config.AutoFillContainerRegistryPaths();
            }

            if (json.Jenkins != null)
            {
                config.CreateJenkinsPipeline = json.Jenkins.CreatePipeline;
                config.JenkinsFolder = json.Jenkins.Folder;
                config.JenkinsProjectFolderName = json.Jenkins.ProjectFolderName ?? config.ProjectSlug;
                config.JenkinsBackendPipelineName = json.Jenkins.BackendPipelineName ?? "backend";
                config.JenkinsFrontendPipelineName = json.Jenkins.FrontendPipelineName ?? "frontend";
                config.JenkinsGitCredentialsId = json.Jenkins.GitCredentialsId ?? "";

                if (json.Jenkins.CreateGitCredentialBackend.HasValue)
                    config.CreateJenkinsGitCredentialBackend = json.Jenkins.CreateGitCredentialBackend.Value;

                if (json.Jenkins.CreateGitCredentialFrontend.HasValue)
                    config.CreateJenkinsGitCredentialFrontend = json.Jenkins.CreateGitCredentialFrontend.Value;

                config.JenkinsExistingGitUrlIdBackend = json.Jenkins.ExistingGitUrlIdBackend;
                config.JenkinsExistingGitUrlIdFrontend = json.Jenkins.ExistingGitUrlIdFrontend;
            }
            else
            {
                config.CreateJenkinsPipeline = false;
                config.JenkinsProjectFolderName = config.ProjectSlug;
            }

            if (json.Database != null)
            {
                config.CreateDatabaseUser = json.Database.CreateUser;

                if (json.Database.Stg != null)
                {
                    config.CreateDatabaseUserStg = json.Database.Stg.Enabled;
                    config.DatabaseUserStg = json.Database.Stg.Username ?? config.GetDefaultDatabaseUser();
                    config.DatabasePasswordStg = json.Database.Stg.Password;
                }
                else
                {
                    config.CreateDatabaseUserStg = false;
                    config.DatabaseUserStg = config.GetDefaultDatabaseUser();
                }

                if (json.Database.Prd != null)
                {
                    config.CreateDatabaseUserPrd = json.Database.Prd.Enabled;
                    config.DatabaseUserPrd = json.Database.Prd.Username ?? config.GetDefaultDatabaseUser();
                    config.DatabasePasswordPrd = json.Database.Prd.Password;
                }
                else
                {
                    config.CreateDatabaseUserPrd = false;
                    config.DatabaseUserPrd = config.GetDefaultDatabaseUser();
                }
            }
            else
            {
                config.CreateDatabaseUser = false;
                config.DatabaseUserStg = config.GetDefaultDatabaseUser();
                config.DatabaseUserPrd = config.GetDefaultDatabaseUser();
            }

            if (json.Docker != null)
            {
                config.GenerateDockerCompose = json.Docker.GenerateCompose;
                config.DeployDockerCompose = json.Docker.DeployCompose;
                config.DeployFolderName = json.Docker.DeployFolderName ?? config.ProjectSlug;
                config.ContainerNameFrontend = json.Docker.ContainerNameFrontend ?? config.GetDefaultContainerNameFrontend();
                config.ContainerNameBackend = json.Docker.ContainerNameBackend ?? config.GetDefaultContainerNameBackend();

                if (json.Docker.Stg != null)
                {
                    config.DeployDockerComposeStg = json.Docker.Stg.Deploy;
                    config.CopyDockerComposeLocalStg = json.Docker.Stg.CopyLocal;
                    config.ReverseProxyHostStg = json.Docker.Stg.ReverseProxyHost ?? "";
                    config.ReverseProxyPathPrefixStg = json.Docker.Stg.ReverseProxyPathPrefix ?? config.GetDefaultReverseProxyPathPrefix();
                    config.ReverseProxyPathPrefixApiStg = json.Docker.Stg.ReverseProxyPathPrefixApi ?? $"{config.ReverseProxyPathPrefixStg}/api";
                    config.LocalDeployPathStg = json.Docker.Stg.LocalDeployPath;
                    config.UseStripPrefixFrontendStg = json.Docker.Stg.UseStripPrefixFrontend;
                    config.UseStripPrefixBackendStg = json.Docker.Stg.UseStripPrefixBackend;
                }
                else
                {
                    config.DeployDockerComposeStg = false;
                    config.CopyDockerComposeLocalStg = false;
                    config.ReverseProxyPathPrefixStg = config.GetDefaultReverseProxyPathPrefix();
                    config.ReverseProxyPathPrefixApiStg = $"{config.ReverseProxyPathPrefixStg}/api";
                }

                if (json.Docker.Prd != null)
                {
                    config.DeployDockerComposePrd = json.Docker.Prd.Deploy;
                    config.CopyDockerComposeLocalPrd = json.Docker.Prd.CopyLocal;
                    config.ReverseProxyHostPrd = json.Docker.Prd.ReverseProxyHost ?? "";
                    config.ReverseProxyPathPrefixPrd = json.Docker.Prd.ReverseProxyPathPrefix ?? config.GetDefaultReverseProxyPathPrefix();
                    config.ReverseProxyPathPrefixApiPrd = json.Docker.Prd.ReverseProxyPathPrefixApi ?? $"{config.ReverseProxyPathPrefixPrd}/api";
                    config.LocalDeployPathPrd = json.Docker.Prd.LocalDeployPath;
                    config.UseStripPrefixFrontendPrd = json.Docker.Prd.UseStripPrefixFrontend;
                    config.UseStripPrefixBackendPrd = json.Docker.Prd.UseStripPrefixBackend;
                }
                else
                {
                    config.DeployDockerComposePrd = false;
                    config.CopyDockerComposeLocalPrd = false;
                    config.ReverseProxyPathPrefixPrd = config.GetDefaultReverseProxyPathPrefix();
                    config.ReverseProxyPathPrefixApiPrd = $"{config.ReverseProxyPathPrefixPrd}/api";
                }
            }
            else
            {
                config.GenerateDockerCompose = false;
                config.DeployDockerCompose = false;
                config.DeployFolderName = config.ProjectSlug;
                config.ContainerNameFrontend = config.GetDefaultContainerNameFrontend();
                config.ContainerNameBackend = config.GetDefaultContainerNameBackend();
                config.ReverseProxyPathPrefixStg = config.GetDefaultReverseProxyPathPrefix();
                config.ReverseProxyPathPrefixPrd = config.GetDefaultReverseProxyPathPrefix();
                config.ReverseProxyPathPrefixApiStg = $"{config.ReverseProxyPathPrefixStg}/api";
                config.ReverseProxyPathPrefixApiPrd = $"{config.ReverseProxyPathPrefixPrd}/api";
            }

            return config;
        }

        public static string GenerateExampleJson()
        {
            var example = new ProjectConfigurationJson
            {
                ProjectName = "Projeto de Exemplo",
                Type = Shared.Enums.ProjectType.Fullstack,
                Description = "Descrição de Exemplo do Projeto",
                FrontendProgrammingLanguage = Shared.Enums.ProgrammingLanguage.NodeJS,
                BackendProgrammingLanguage = Shared.Enums.ProgrammingLanguage.NodeJS,
                Git = new GitConfigurationJson
                {
                    CreateRepository = true,
                    InitializeWithReadme = false,
                    Namespace = "",
                    RepositoryName = ""
                },
                ContainerRegistry = new ContainerRegistryConfigurationJson
                {
                    CreateProject = true,
                    ProjectName = ""
                },
                Jenkins = new JenkinsConfigurationJson
                {
                    CreatePipeline = true,
                    Folder = "",
                    ProjectFolderName = "",
                    BackendPipelineName = "backend",
                    FrontendPipelineName = "frontend",
                    GitCredentialsId = ""
                },
                Database = new DatabaseConfigurationJson
                {
                    CreateUser = true,
                    Stg = new DatabaseEnvironmentConfigurationJson
                    {
                        Enabled = true,
                        Username = "usuario-exemplo",
                        Password = "senha-exemplo"
                    },
                    Prd = new DatabaseEnvironmentConfigurationJson
                    {
                        Enabled = true,
                        Username = "usuario-exemplo",
                        Password = "senha-exemplo"
                    }
                },
                Docker = new DockerConfigurationJson
                {
                    GenerateCompose = true,
                    DeployCompose = true,
                    DeployFolderName = "projeto-exemplo",
                    ContainerNameFrontend = "projeto-exemplo-frontend",
                    ContainerNameBackend = "projeto-exemplo-backend",
                    Stg = new DockerEnvironmentConfigurationJson
                    {
                        Deploy = true,
                        CopyLocal = true,
                        ReverseProxyHost = "projeto-exemplo.com.br",
                        ReverseProxyPathPrefix = "/projeto-exemplo",
                        ReverseProxyPathPrefixApi = "/projeto-exemplo/api",
                        LocalDeployPath = @"C:\Caminho\Fisico\Projeto\Exemplo",
                        UseStripPrefixFrontend = false,
                        UseStripPrefixBackend = true
                    },
                    Prd = new DockerEnvironmentConfigurationJson
                    {
                        Deploy = true,
                        CopyLocal = true,
                        ReverseProxyHost = "projeto-exemplo.com.br",
                        ReverseProxyPathPrefix = "/projeto-exemplo",
                        ReverseProxyPathPrefixApi = "/projeto-exemplo/api",
                        LocalDeployPath = @"C:\Caminho\Fisico\Projeto\Exemplo",
                        UseStripPrefixFrontend = false,
                        UseStripPrefixBackend = true
                    }
                }
            };

            return JsonSerializer.Serialize(example, jsonSerializerOptions);
        }
    }
}