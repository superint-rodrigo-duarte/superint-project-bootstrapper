namespace superint.ProjectBootstrapper.Shared.Constants
{
    public static class ApiPaths
    {
        public static class GitLab
        {
            public const string ApiVersion = "api/v4";
            public const string User = "user";
            public const string Projects = "projects";
            public const string Namespaces = "namespaces";
        }

        public static class Harbor
        {
            public const string ApiVersion = "api/v2.0";
            public const string CurrentUser = "users/current";
            public const string Projects = "projects";
            public const string Retentions = "retentions";
            public const string ProjectMembers = "members";
        }

        public static class Jenkins
        {
            public const string ApiJson = "api/json";
            public const string CreateItem = "createItem";
            public const string Build = "build";
        }
    }
}