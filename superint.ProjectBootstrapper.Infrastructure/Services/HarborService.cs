using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.ApiResponses.ContainerRegistry;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;
using superint.ProjectBootstrapper.Shared.Constants;
using superint.ProjectBootstrapper.Shared.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class HarborService : IContainerRegistryService
    {
        private readonly ContainerRegistrySettings _containerRegistrySettings;
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClient _httpClient;
        private readonly Uri _baseUri;
        private string? _csrfToken;

        public HarborService(ContainerRegistrySettings containerRegistrySettings)
        {
            _containerRegistrySettings = containerRegistrySettings;
            _cookieContainer = new CookieContainer();

            var baseUrl = containerRegistrySettings.Url.StartsWith("http") ? containerRegistrySettings.Url : $"https://{containerRegistrySettings.Url}";
            _baseUri = new Uri(baseUrl.TrimEnd('/') + "/" + ApiPaths.Harbor.ApiVersion + "/");

            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = _baseUri
            };

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{containerRegistrySettings.Username}:{containerRegistrySettings.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void ExtractCsrfToken(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.Headers.TryGetValues("X-Harbor-CSRF-Token", out var headerValues))
            {
                _csrfToken = headerValues.FirstOrDefault();
                return;
            }

            var cookies = _cookieContainer.GetCookies(_baseUri);
            var possibleCookieNames = new[] { "_gorilla_csrf", "csrf_token", "_xsrf", "harbor.csrf.token", "sid" };

            foreach (var cookieName in possibleCookieNames)
            {
                var cookie = cookies[cookieName];
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    _csrfToken = Uri.UnescapeDataString(cookie.Value);
                    return;
                }
            }
        }

        private async Task<string?> GetCsrfTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_csrfToken))
                return _csrfToken;

            var response = await _httpClient.GetAsync(ApiPaths.Harbor.CurrentUser, cancellationToken);
            ExtractCsrfToken(response);

            return _csrfToken;
        }

        private async Task<HttpResponseMessage> PostWithCsrfAsync<T>(string requestUri, T content, CancellationToken cancellationToken)
        {
            var csrfToken = await GetCsrfTokenAsync(cancellationToken);

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequestMessage.Content = JsonContent.Create(content, options: JsonSerializationHelper.SnakeCaseApiOptions);

            if (!string.IsNullOrEmpty(csrfToken))
                httpRequestMessage.Headers.Add("X-Harbor-CSRF-Token", csrfToken);

            return await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        }

        private async Task<bool> ConfigureRetentionPolicyAsync(string projectName, CancellationToken cancellationToken)
        {
            try
            {
                var projectHttpResponseMessage = await _httpClient.GetAsync($"{ApiPaths.Harbor.Projects}?name={Uri.EscapeDataString(projectName)}", cancellationToken);

                if (!projectHttpResponseMessage.IsSuccessStatusCode) return false;

                var dtoContainerRegistryProjectsResponse = await projectHttpResponseMessage.Content.ReadFromJsonAsync<List<ContainerRegistryProjectResponse>>(JsonSerializationHelper.SnakeCaseApiOptions, cancellationToken);
                var dtoContainerRegistryProjectResponse = dtoContainerRegistryProjectsResponse?.FirstOrDefault(T => T.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
                if (dtoContainerRegistryProjectResponse == null) return false;

                var dtoContainerRegistryRetentionPolicySettings = _containerRegistrySettings.RetentionPolicy;

                var rules = new List<object>
                {
                    new
                    {
                        disabled = false,
                        action = "retain",
                        @params = new Dictionary<string, int> { ["latestPushedK"] = dtoContainerRegistryRetentionPolicySettings.RetainCount },
                        scope_selectors = new
                        {
                            repository = new[] { new { kind = "doublestar", decoration = "repoMatches", pattern = dtoContainerRegistryRetentionPolicySettings.RepositoryPattern } }
                        },
                        tag_selectors = new[] { new { kind = "doublestar", decoration = "matches", pattern = dtoContainerRegistryRetentionPolicySettings.TagPattern } }
                    }
                };

                if (dtoContainerRegistryRetentionPolicySettings.IncludeUntagged)
                {
                    rules.Add(new
                    {
                        disabled = false,
                        action = "retain",
                        @params = new Dictionary<string, int> { ["latestPushedK"] = dtoContainerRegistryRetentionPolicySettings.RetainCount },
                        scope_selectors = new
                        {
                            repository = new[] { new { kind = "doublestar", decoration = "repoMatches", pattern = dtoContainerRegistryRetentionPolicySettings.RepositoryPattern } }
                        },
                        tag_selectors = new[] { new { kind = "doublestar", decoration = "withUntaggedArtifacts", pattern = dtoContainerRegistryRetentionPolicySettings.TagPattern } }
                    });
                }

                var retentionPolicy = new
                {
                    algorithm = "or",
                    scope = new { level = "project", @ref = dtoContainerRegistryProjectResponse.ProjectId },
                    trigger = new { kind = "Schedule", settings = new { cron = InfrastructureConstants.Harbor.DailyRetentionCron } },
                    rules
                };

                var retentionHttpResponseMessage = await PostWithCsrfAsync(ApiPaths.Harbor.Retentions, retentionPolicy, cancellationToken);
                return retentionHttpResponseMessage.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<string>> AddProjectAdminUsersAsync(string projectName, CancellationToken cancellationToken)
        {
            var addedUsers = new List<string>();

            if (_containerRegistrySettings.ProjectAdminUsers.Count == 0)
                return addedUsers;

            foreach (var username in _containerRegistrySettings.ProjectAdminUsers)
            {
                try
                {
                    var success = await AddProjectMemberAsync(projectName, username, InfrastructureConstants.Harbor.RoleProjectAdmin, cancellationToken);
                    if (success)
                    {
                        addedUsers.Add(username);
                    }
                }
                catch
                {
                }
            }

            return addedUsers;
        }

        private async Task<bool> AddProjectMemberAsync(string projectName, string username, int roleId, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new { role_id = roleId, member_user = new { username } };

                var endpoint = $"{ApiPaths.Harbor.Projects}/{Uri.EscapeDataString(projectName)}/{ApiPaths.Harbor.ProjectMembers}";
                var httpResponseMessage = await PostWithCsrfAsync(endpoint, payload, cancellationToken);

                return httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode == HttpStatusCode.Created;
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var httpResponseMessage = await _httpClient.GetAsync(ApiPaths.Harbor.CurrentUser, cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    ExtractCsrfToken(httpResponseMessage);

                    var httpResponseMessageContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    var dtoContainerRegistryUserResponse = JsonSerializer.Deserialize<ContainerRegistryUserResponse>(httpResponseMessageContent, JsonSerializationHelper.SnakeCaseApiOptions);
                    return OperationResult.Ok($"Conectado como {dtoContainerRegistryUserResponse?.Username}", _containerRegistrySettings.Url);
                }

                return OperationResult.Fail($"Falha ao conectar: {httpResponseMessage.StatusCode}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail("Falha ao conectar ao Harbor", ex);
            }
        }

        public async Task<OperationResult> CreateProjectAsync(string projectName, int retentionDays = 30, CancellationToken cancellationToken = default)
        {
            try
            {
                var checkResponse = await _httpClient.GetAsync($"{ApiPaths.Harbor.Projects}?name={Uri.EscapeDataString(projectName)}", cancellationToken);

                ExtractCsrfToken(checkResponse);

                if (checkResponse.IsSuccessStatusCode)
                {
                    var dtoContainerRegistryProjectsResponse = await checkResponse.Content.ReadFromJsonAsync<List<ContainerRegistryProjectResponse>>(JsonSerializationHelper.SnakeCaseApiOptions, cancellationToken);
                    if (dtoContainerRegistryProjectsResponse?.Any(project => project.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        return OperationResult.Fail($"Projeto '{projectName}' já existe no Harbor");
                    }
                }

                var payload = new
                {
                    project_name = projectName,
                    @public = false,
                    metadata = new { auto_scan = "true", severity = "high" }
                };

                var httpResponseMessage = await PostWithCsrfAsync(ApiPaths.Harbor.Projects, payload, cancellationToken);

                if (httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode == HttpStatusCode.Created)
                {
                    var details = new List<string>();

                    if (_containerRegistrySettings.RetentionPolicy.Enabled)
                    {
                        var retentionConfigured = await ConfigureRetentionPolicyAsync(projectName, cancellationToken);
                        if (retentionConfigured)
                            details.Add($"Retenção: {_containerRegistrySettings.RetentionPolicy.RetainCount} últimos artefatos");
                    }

                    var addedUsers = await AddProjectAdminUsersAsync(projectName, cancellationToken);
                    if (addedUsers.Count > 0)
                        details.Add($"Admins: {string.Join(", ", addedUsers)}");

                    return OperationResult.Ok($"Projeto '{projectName}' criado no Harbor", details.Count > 0 ? string.Join(" | ", details) : "Projeto criado com sucesso");
                }

                var httpResponseMessageErrorContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                return OperationResult.Fail($"Falha ao criar projeto Harbor: {httpResponseMessage.StatusCode} - {httpResponseMessageErrorContent}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao criar projeto Harbor '{projectName}'", ex);
            }
        }
    }
}