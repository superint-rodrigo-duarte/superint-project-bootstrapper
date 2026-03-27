namespace superint.ProjectBootstrapper.DTO.ApiResponses.GitLab
{
    public sealed class GitLabProjectResponse
    {
        public int Id { get; set; }
        public string WebUrl { get; set; } = string.Empty;
    }
}