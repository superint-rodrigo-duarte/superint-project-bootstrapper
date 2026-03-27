namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class JenkinsSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
        public string DefaultFolder { get; set; } = string.Empty;
        public int MaxBuildsToKeep { get; set; } = 5;
        public string[] Environments { get; set; } = [];
        public string CredentialsId { get; set; } = "gitlab-credentials";
        public string DefaultBranch { get; set; } = "main";
        public string JenkinsfilePath { get; set; } = "Jenkinsfile";
    }
}