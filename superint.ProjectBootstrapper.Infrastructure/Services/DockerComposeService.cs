using Scriban;
using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Helpers;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;
using superint.ProjectBootstrapper.Shared.Constants;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class DockerComposeService(DeploymentSettings deploymentSettings, ContainerRegistrySettings containerRegistrySettings) : IDockerComposeService
    {
        private static string GetDockerComposeTemplateName(ProjectType projectType)
        {
            return projectType switch
            {
                ProjectType.Frontend => "docker-compose.frontend.sbn",
                ProjectType.Backend => "docker-compose.backend.sbn",
                ProjectType.Fullstack => "docker-compose.fullstack.sbn",
                _ => throw new ArgumentOutOfRangeException(nameof(projectType), projectType, "Tipo de projeto não suportado")
            };
        }

        public Task<OperationResult> GenerateDockerComposeAsync(ProjectConfiguration dtoProjectConfiguration, string environment, string dockerComposeOutputPath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var content = GenerateDockerComposeContent(dtoProjectConfiguration, environment);

                var dockerComposeFileName = "docker-compose.yml";
                var dockerComposeFullPath = Path.Combine(dockerComposeOutputPath, dockerComposeFileName);

                Directory.CreateDirectory(dockerComposeOutputPath);
                File.WriteAllText(dockerComposeFullPath, content);

                return Task.FromResult(OperationResult.Ok($"Docker Compose gerado para {environment.ToUpperInvariant()}", $"Arquivo: {dockerComposeFullPath}"));
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(OperationResult.Fail("Operação cancelada"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(OperationResult.Fail($"Falha ao gerar Docker Compose para {environment}", ex));
            }
        }

        public string GenerateDockerComposeContent(ProjectConfiguration dtoProjectConfiguration, string environment)
        {
            var dockerComposeTemplateName = GetDockerComposeTemplateName(dtoProjectConfiguration.ProjectType);
            var dockerComposeTemplateContent = TemplateLoader.LoadDockerComposeTemplateContent(dockerComposeTemplateName);

            var dockerComposeTemplateContentParsed = Template.Parse(dockerComposeTemplateContent);
            if (dockerComposeTemplateContentParsed.HasErrors)
                throw new InvalidOperationException($"Erro ao parsear template: {string.Join(", ", dockerComposeTemplateContentParsed.Messages)}");

            var isProduction = environment.Equals(InfrastructureConstants.Environments.Production, StringComparison.OrdinalIgnoreCase);
            var isCSharpBackend = dtoProjectConfiguration.BackendProgrammingLanguage == ProgrammingLanguage.CSharp;

            var model = new
            {
                deploy_folder_name = dtoProjectConfiguration.DeployFolderName,
                project_slug = dtoProjectConfiguration.ProjectSlug,
                project_name = dtoProjectConfiguration.ProjectName,
                network_name = deploymentSettings.NetworkName,
                container_registry_url = containerRegistrySettings.Url,
                registry_path_backend = isProduction ? dtoProjectConfiguration.ContainerRegistryPathBackendPrd : dtoProjectConfiguration.ContainerRegistryPathBackendStg,
                registry_path_frontend = isProduction ? dtoProjectConfiguration.ContainerRegistryPathFrontendPrd : dtoProjectConfiguration.ContainerRegistryPathFrontendStg,
                is_production = isProduction,
                ReverseProxy_entrypoint = isProduction ? InfrastructureConstants.ReverseProxy.SecureEntrypoint : InfrastructureConstants.ReverseProxy.InsecureEntrypoint,
                ReverseProxy_host = isProduction ? dtoProjectConfiguration.ReverseProxyHostPrd : dtoProjectConfiguration.ReverseProxyHostStg,
                ReverseProxy_path_prefix = isProduction ? dtoProjectConfiguration.ReverseProxyPathPrefixPrd : dtoProjectConfiguration.ReverseProxyPathPrefixStg,
                ReverseProxy_path_prefix_api = isProduction ? dtoProjectConfiguration.ReverseProxyPathPrefixApiPrd : dtoProjectConfiguration.ReverseProxyPathPrefixApiStg,
                container_name_frontend = dtoProjectConfiguration.ContainerNameFrontend,
                container_name_backend = dtoProjectConfiguration.ContainerNameBackend,
                frontend_port = ProjectConfiguration.FrontendPort,
                backend_port = dtoProjectConfiguration.BackendPort,
                is_csharp_backend = isCSharpBackend,
                aspnetcore_environment = isProduction ? InfrastructureConstants.AspNetCore.ProductionEnvironment : InfrastructureConstants.AspNetCore.StagingEnvironment,
                use_strip_prefix_frontend = isProduction ? dtoProjectConfiguration.UseStripPrefixFrontendPrd : dtoProjectConfiguration.UseStripPrefixFrontendStg,
                use_strip_prefix_backend = isProduction ? dtoProjectConfiguration.UseStripPrefixBackendPrd : dtoProjectConfiguration.UseStripPrefixBackendStg
            };

            return dockerComposeTemplateContentParsed.Render(model, memberRenamer: member => member.Name);
        }
    }
}