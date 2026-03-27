namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class DatabaseSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string AdminUser { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
        public string DefaultDatabase { get; set; } = "postgres";
    }
}