using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.ApiResponses.GitHub;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;
using superint.ProjectBootstrapper.Shared.Helpers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class GitHubService(GitSettings gitSettings) : IGitService
    {
        private readonly HttpClient _httpClient = CreateHttpClient(gitSettings);

        private static HttpClient CreateHttpClient(GitSettings settings)
        {
            var HttpClient = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiToken);
            HttpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            HttpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "ProjectBootstrapper");
            return HttpClient;
        }

        private async Task<string> GetAuthenticatedUserAsync(CancellationToken cancellationToken)
        {
            var httpResponseMessage = await _httpClient.GetAsync("user", cancellationToken);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                var dtoGitHubUserResponse = JsonSerializer.Deserialize<GitHubUserResponse>(content, JsonSerializationHelper.DefaultApiOptions);
                return dtoGitHubUserResponse?.Login ?? string.Empty;
            }
            return string.Empty;
        }

        public async Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var content = string.Empty;

                var httpResponseMessage = await _httpClient.GetAsync("user", cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    var dtoGitHubUserResponse = JsonSerializer.Deserialize<GitHubUserResponse>(content, JsonSerializationHelper.DefaultApiOptions);
                    return OperationResult.Ok($"Conectado como {dtoGitHubUserResponse?.Login}", "https://github.com");
                }

                content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.Fail($"Falha ao conectar: {httpResponseMessage.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail("Falha ao conectar ao GitHub", ex);
            }
        }

        public async Task<OperationResult> CreateRepositoryAsync(string repositoryName, string description, string? namespacePath = null, bool autoInit = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var content = string.Empty;

                var targetOrg = namespacePath ?? gitSettings.DefaultNamespace;
                var isOrgRepository = !string.IsNullOrEmpty(targetOrg);

                // Verificar se o repositório já existe
                var checkEndpoint = isOrgRepository ? $"repos/{targetOrg}/{repositoryName}" : $"repos/{await GetAuthenticatedUserAsync(cancellationToken)}/{repositoryName}";
                var checkHttpResponseMessage = await _httpClient.GetAsync(checkEndpoint, cancellationToken);
                if (checkHttpResponseMessage.IsSuccessStatusCode)
                    return OperationResult.Fail($"Repositório '{repositoryName}' já existe");

                var endpoint = isOrgRepository ? $"orgs/{targetOrg}/repos" : "user/repos";

                var payload = new
                {
                    name = repositoryName,
                    description,
                    @private = true,
                    auto_init = autoInit,
                    has_issues = true,
                    has_projects = false,
                    has_wiki = false
                };

                var httpResponseMessage = await _httpClient.PostAsJsonAsync(endpoint, payload, cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    var dtoGitHubRepositoryResponse = JsonSerializer.Deserialize<GitHubRepositoryResponse>(content, JsonSerializationHelper.DefaultApiOptions);
                    return OperationResult.Ok($"Repositório '{repositoryName}' criado", $"URL: {dtoGitHubRepositoryResponse?.HtmlUrl}");
                }

                content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity && content.Contains("name already exists"))
                    return OperationResult.Fail($"Repositório '{repositoryName}' já existe");

                return OperationResult.Fail($"Falha ao criar repositório: {httpResponseMessage.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao criar repositório '{repositoryName}'", ex);
            }
        }
    }
}