using Scriban;
using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Helpers;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;
using superint.ProjectBootstrapper.Shared.Constants;
using superint.ProjectBootstrapper.Shared.Enums;
using superint.ProjectBootstrapper.Shared.Extensions;
using System.Net.Http.Headers;
using System.Text;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class JenkinsService(JenkinsSettings dtoJenkinsSettings) : IJenkinsService
    {
        private readonly HttpClient _httpClient = CreateHttpClient(dtoJenkinsSettings);

        private static HttpClient CreateHttpClient(JenkinsSettings dtoJenkinsSettingsInternal)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(dtoJenkinsSettingsInternal.BaseUrl.TrimEnd('/') + "/")
            };

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{dtoJenkinsSettingsInternal.Username}:{dtoJenkinsSettingsInternal.ApiToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            return httpClient;
        }

        private static string RenderFolderConfig(string folderName)
        {
            var templateContent = TemplateLoader.LoadJenkinsTemplate("jenkins-folder.xml.sbn");
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
                throw new InvalidOperationException($"Erro ao parsear template de folder: {string.Join(", ", template.Messages)}");

            var model = new { display_name = folderName.ToPascalCase(), description = $"Pasta gerada automaticamente para {folderName}" };

            return template.Render(model, memberRenamer: member => member.Name);
        }

        private string RenderPipelineConfig(string pipelineName, string pipelineScript)
        {
            var templateContent = TemplateLoader.LoadJenkinsTemplate("jenkins-pipeline.xml.sbn");
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
                throw new InvalidOperationException($"Erro ao parsear template de pipeline: {string.Join(", ", template.Messages)}");

            var model = new
            {
                description = $"Pipeline gerado automaticamente para {pipelineName}",
                max_builds_to_keep = dtoJenkinsSettings.MaxBuildsToKeep,
                environments = dtoJenkinsSettings.Environments,
                pipeline_script = EscapeXmlContent(pipelineScript)
            };

            return template.Render(model, memberRenamer: member => member.Name);
        }

        private static string RenderJenkinsfileTemplate(JenkinsPipelineConfiguration dtoJenkinsPipelineConfiguration)
        {
            var templateName = GetJenkinsfileTemplateName(dtoJenkinsPipelineConfiguration.PipelineStackType, dtoJenkinsPipelineConfiguration.ProgrammingLanguage);
            var templateContent = TemplateLoader.LoadJenkinsfileTemplate(templateName);
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
                throw new InvalidOperationException($"Erro ao parsear template de Jenkinsfile: {string.Join(", ", template.Messages)}");

            var model = new
            {
                app_name = dtoJenkinsPipelineConfiguration.ApplicationName,
                deploy_folder_name = dtoJenkinsPipelineConfiguration.DeployFolderName,
                project_name = dtoJenkinsPipelineConfiguration.DotNetProjectName ?? dtoJenkinsPipelineConfiguration.ApplicationName,
                git_url_id = dtoJenkinsPipelineConfiguration.GitUrlId,
                git_credentials_id = dtoJenkinsPipelineConfiguration.GitCredentialsId,
                container_registry_path_stg = dtoJenkinsPipelineConfiguration.ContainerRegistryPathStg,
                container_registry_path_prd = dtoJenkinsPipelineConfiguration.ContainerRegistryPathPrd,
                ReverseProxy_path_prefix_stg = dtoJenkinsPipelineConfiguration.ReverseProxyPathPrefixStg,
                ReverseProxy_path_prefix_prd = dtoJenkinsPipelineConfiguration.ReverseProxyPathPrefixPrd
            };

            return template.Render(model, memberRenamer: member => member.Name);
        }

        private static string GetJenkinsfileTemplateName(PipelineStackType pipelineStackType, ProgrammingLanguage programmingLanguage)
        {
            return pipelineStackType switch
            {
                PipelineStackType.Frontend => "Jenkinsfile.frontend.nodejs.sbn",
                PipelineStackType.Backend => programmingLanguage switch
                {
                    ProgrammingLanguage.CSharp => "Jenkinsfile.backend.csharp.sbn",
                    ProgrammingLanguage.NodeJS => "Jenkinsfile.backend.nodejs.sbn",
                    _ => "Jenkinsfile.backend.nodejs.sbn"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(pipelineStackType), $"StackType não suportado: {pipelineStackType}")
            };
        }

        private static string EscapeXmlContent(string content)
        {
            return content.Replace("&", "&amp;")
                          .Replace("<", "&lt;")
                          .Replace(">", "&gt;")
                          .Replace("\"", "&quot;")
                          .Replace("'", "&apos;");
        }

        private static bool IsJsonResponse(HttpResponseMessage httpResponseMessage)
        {
            var contentType = httpResponseMessage.Content.Headers.ContentType?.MediaType;
            return contentType != null && contentType.Contains("json", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractErrorMessage(string content, string statusCode)
        {
            if (!content.TrimStart().StartsWith("<", StringComparison.Ordinal))
                return $"{statusCode} - {content}";

            var errorIndex = content.IndexOf("<h1>Error</h1>", StringComparison.OrdinalIgnoreCase);
            if (errorIndex >= 0)
            {
                var pStart = content.IndexOf("<p>", errorIndex, StringComparison.OrdinalIgnoreCase);
                if (pStart >= 0)
                {
                    pStart += 3;
                    var pEnd = content.IndexOf("</p>", pStart, StringComparison.OrdinalIgnoreCase);
                    if (pEnd < 0) pEnd = content.IndexOf("<", pStart, StringComparison.OrdinalIgnoreCase);
                    if (pEnd > pStart)
                    {
                        return content[pStart..pEnd].Trim();
                    }
                }
            }

            return statusCode;
        }

        public async Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var httpResponseMessage = await _httpClient.GetAsync(ApiPaths.Jenkins.ApiJson, cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return OperationResult.Ok("Conectado ao Jenkins", dtoJenkinsSettings.BaseUrl);

                return OperationResult.Fail($"Falha ao conectar: {httpResponseMessage.StatusCode}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail("Falha ao conectar ao Jenkins", ex);
            }
        }

        public async Task<OperationResult> CreateSecretTextCredentialAsync(string credentialId, string credentialName, string secretValue, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage httpResponseMessage;

                httpResponseMessage = await _httpClient.GetAsync($"credentials/store/system/domain/_/credential/{Uri.EscapeDataString(credentialId)}/api/json", cancellationToken);

                if (httpResponseMessage.IsSuccessStatusCode && IsJsonResponse(httpResponseMessage))
                    return OperationResult.Ok($"Credencial '{credentialId}' já existe", "Usando credencial existente");

                var credentialXml = $@"<org.jenkinsci.plugins.plaincredentials.impl.StringCredentialsImpl>
                                       <scope>GLOBAL</scope>
                                       <id>{EscapeXmlContent(credentialId)}</id>
                                       <description>{EscapeXmlContent(credentialName)}</description>
                                       <secret>{EscapeXmlContent(secretValue)}</secret>
                                       </org.jenkinsci.plugins.plaincredentials.impl.StringCredentialsImpl>";

                var content = new StringContent(credentialXml, Encoding.UTF8, "application/xml");

                httpResponseMessage = await _httpClient.PostAsync("credentials/store/system/domain/_/createCredentials", content, cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return OperationResult.Ok($"Credencial '{credentialId}' criada", credentialName);

                var errorContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (errorContent.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return OperationResult.Ok($"Credencial '{credentialId}' já existe", "Usando credencial existente");

                var errorHttpResponseMessage = ExtractErrorMessage(errorContent, httpResponseMessage.StatusCode.ToString());
                return OperationResult.Fail($"Falha ao criar credencial: {errorHttpResponseMessage}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao criar credencial Jenkins '{credentialId}'", ex);
            }
        }

        public async Task<OperationResult> CreateFolderAsync(string folderName, string? parentFolder = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var targetFolder = parentFolder ?? dtoJenkinsSettings.DefaultFolder;
                var basePath = string.IsNullOrEmpty(targetFolder) ? "" : $"job/{targetFolder}/";

                var checkResponse = await _httpClient.GetAsync($"{basePath}job/{folderName}/{ApiPaths.Jenkins.ApiJson}", cancellationToken);

                if (checkResponse.IsSuccessStatusCode && IsJsonResponse(checkResponse))
                    return OperationResult.Ok($"Pasta '{folderName}' já existe", "Usando pasta existente");

                var folderConfig = RenderFolderConfig(folderName);
                var content = new StringContent(folderConfig, Encoding.UTF8, "application/xml");

                var httpResponseMessage = await _httpClient.PostAsync($"{basePath}{ApiPaths.Jenkins.CreateItem}?name={Uri.EscapeDataString(folderName)}", content, cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var fullPath = string.IsNullOrEmpty(targetFolder) ? folderName : $"{targetFolder}/{folderName}";
                    return OperationResult.Ok($"Pasta '{folderName}' criada", $"Caminho: {fullPath}");
                }

                var errorHttpResponseMessage = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (errorHttpResponseMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return OperationResult.Ok($"Pasta '{folderName}' já existe", "Usando pasta existente");

                var errorMessage = ExtractErrorMessage(errorHttpResponseMessage, httpResponseMessage.StatusCode.ToString());
                return OperationResult.Fail($"Falha ao criar pasta: {errorMessage}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao criar pasta Jenkins '{folderName}'", ex);
            }
        }

        public async Task<OperationResult> CreatePipelineAsync(JenkinsPipelineConfiguration dtoJenkinsPipelineConfiguration, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage httpResponseMessage;

                var targetFolder = dtoJenkinsPipelineConfiguration.Folder ?? dtoJenkinsSettings.DefaultFolder;
                var basePath = string.IsNullOrEmpty(targetFolder) ? "" : $"job/{targetFolder}/";

                httpResponseMessage = await _httpClient.GetAsync($"{basePath}job/{dtoJenkinsPipelineConfiguration.PipelineName}/{ApiPaths.Jenkins.ApiJson}", cancellationToken);

                if (httpResponseMessage.IsSuccessStatusCode && IsJsonResponse(httpResponseMessage))
                    return OperationResult.Ok($"Pipeline '{dtoJenkinsPipelineConfiguration.PipelineName}' já existe", "Usando pipeline existente");

                var jenkinsfileContent = RenderJenkinsfileTemplate(dtoJenkinsPipelineConfiguration);

                var pipelineXml = RenderPipelineConfig(dtoJenkinsPipelineConfiguration.PipelineName, jenkinsfileContent);
                var content = new StringContent(pipelineXml, Encoding.UTF8, "application/xml");

                httpResponseMessage = await _httpClient.PostAsync($"{basePath}{ApiPaths.Jenkins.CreateItem}?name={Uri.EscapeDataString(dtoJenkinsPipelineConfiguration.PipelineName)}", content, cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var fullPath = string.IsNullOrEmpty(targetFolder) ? dtoJenkinsPipelineConfiguration.PipelineName : $"{targetFolder}/{dtoJenkinsPipelineConfiguration.PipelineName}";
                    return OperationResult.Ok($"Pipeline '{dtoJenkinsPipelineConfiguration.PipelineName}' criado", $"Caminho: {fullPath}");
                }

                var errorHttpResponseMessage = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (errorHttpResponseMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return OperationResult.Ok($"Pipeline '{dtoJenkinsPipelineConfiguration.PipelineName}' já existe", "Usando pipeline existente");

                var errorMessage = ExtractErrorMessage(errorHttpResponseMessage, httpResponseMessage.StatusCode.ToString());
                return OperationResult.Fail($"Falha ao criar pipeline: {errorMessage}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao criar pipeline Jenkins '{dtoJenkinsPipelineConfiguration.PipelineName}'", ex);
            }
        }
    }
}