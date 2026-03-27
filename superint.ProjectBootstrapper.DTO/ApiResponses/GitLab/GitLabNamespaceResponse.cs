namespace superint.ProjectBootstrapper.DTO.ApiResponses.GitLab
{
    public sealed class GitLabNamespaceResponse
    {
        public int Id { get; set; }
        public string FullPath { get; set; } = string.Empty;
    }
}