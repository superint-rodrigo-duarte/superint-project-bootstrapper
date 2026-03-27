namespace superint.ProjectBootstrapper.Shared.Constants
{
    public static class InfrastructureConstants
    {
        public static class Docker
        {
            public const string ComposeUpdateCommand = "docker compose pull && docker compose up -d";
            public const string ComposeStopCommand = "docker compose down";
            public const string ComposeLogsCommand = "docker compose logs -f";
        }

        public static class Environments
        {
            public const string ProductionTag = "main";
            public const string StagingTag = "staging";
            public const string Production = "prd";
            public const string Staging = "stg";
        }

        public static class Harbor
        {
            public const string DailyRetentionCron = "0 0 0 * * *";
            public const int RoleProjectAdmin = 1;
            public const int RoleDeveloper = 2;
            public const int RoleGuest = 3;
            public const int RoleMaintainer = 4;
        }

        public static class AspNetCore
        {
            public const string ProductionEnvironment = "Production";
            public const string StagingEnvironment = "Staging";
            public const string DevelopmentEnvironment = "Development";
        }

        public static class ReverseProxy
        {
            public const string SecureEntrypoint = "websecure";
            public const string InsecureEntrypoint = "web";
        }
    }
}