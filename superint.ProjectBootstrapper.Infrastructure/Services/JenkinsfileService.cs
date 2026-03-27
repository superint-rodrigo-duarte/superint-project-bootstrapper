using Scriban;
using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Infrastructure.Helpers;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class JenkinsfileService : IJenkinsfileService
    {
        private static string GetTemplateName(ProjectConfiguration dtoProjectConfiguration, StackType stackType)
        {
            if (stackType == StackType.Frontend)
            {
                return "Jenkinsfile.frontend.nodejs.sbn";
            }

            return dtoProjectConfiguration.BackendProgrammingLanguage switch
            {
                ProgrammingLanguage.CSharp => "Jenkinsfile.backend.csharp.sbn",
                ProgrammingLanguage.NodeJS => "Jenkinsfile.backend.nodejs.sbn",
                _ => throw new ArgumentOutOfRangeException(nameof(dtoProjectConfiguration.BackendProgrammingLanguage), dtoProjectConfiguration.BackendProgrammingLanguage, "Linguagem não suportada")
            };
        }

        private static string GetGitUrlId(ProjectConfiguration dtoProjectConfiguration, StackType stackType)
        {
            var repoName = stackType == StackType.Frontend ? dtoProjectConfiguration.FrontendRepositoryName : dtoProjectConfiguration.BackendRepositoryName;
            return $"superint-{repoName}";
        }

        public string GenerateJenkinsfile(ProjectConfiguration dtoProjectConfiguration, StackType stackType)
        {
            var templateName = GetTemplateName(dtoProjectConfiguration, stackType);
            var templateContent = TemplateLoader.LoadJenkinsfileTemplate(templateName);

            var template = Template.Parse(templateContent);
            if (template.HasErrors)
            {
                throw new InvalidOperationException(
                    $"Erro ao parsear template Jenkinsfile: {string.Join(", ", template.Messages)}");
            }

            var isCSharp = stackType == StackType.Backend && dtoProjectConfiguration.BackendProgrammingLanguage == ProgrammingLanguage.CSharp;

            var model = new
            {
                app_name = dtoProjectConfiguration.ProjectSlug,
                deploy_folder_name = dtoProjectConfiguration.DeployFolderName,
                ReverseProxy_path_prefix_stg = dtoProjectConfiguration.ReverseProxyPathPrefixStg,
                ReverseProxy_path_prefix_prd = dtoProjectConfiguration.ReverseProxyPathPrefixPrd,
                git_url_id = GetGitUrlId(dtoProjectConfiguration, stackType),
                git_credentials_id = dtoProjectConfiguration.JenkinsGitCredentialsId,
                container_registry_path_stg = stackType == StackType.Backend ? dtoProjectConfiguration.ContainerRegistryPathBackendStg : dtoProjectConfiguration.ContainerRegistryPathFrontendStg,
                container_registry_path_prd = stackType == StackType.Backend ? dtoProjectConfiguration.ContainerRegistryPathBackendPrd : dtoProjectConfiguration.ContainerRegistryPathFrontendPrd,
                dotnet_project_name = dtoProjectConfiguration.DotNetProjectName ?? dtoProjectConfiguration.ProjectName.Replace(" ", "").Replace("-", "")
            };

            return template.Render(model, memberRenamer: member => member.Name);
        }

        public Task<OperationResult> SaveJenkinsfileAsync(ProjectConfiguration dtoProjectConfiguration, StackType stackType, string outputPath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var content = GenerateJenkinsfile(dtoProjectConfiguration, stackType);
                var fullPath = Path.Combine(outputPath, "Jenkinsfile");

                Directory.CreateDirectory(outputPath);
                File.WriteAllText(fullPath, content);

                return Task.FromResult(OperationResult.Ok($"Jenkinsfile gerado para {stackType}", $"Arquivo: {fullPath}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(OperationResult.Fail($"Falha ao gerar Jenkinsfile para {stackType}", ex));
            }
        }
    }
}