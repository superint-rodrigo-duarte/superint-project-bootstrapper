namespace superint.ProjectBootstrapper.DTO.ApiResponses.GitHub
{
    public sealed class GitHubRepositoryResponse
    {
        public int Id { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public string CloneUrl { get; set; } = string.Empty;
    }
}