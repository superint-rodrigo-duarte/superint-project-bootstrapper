namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class ServerSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 22;
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? PrivateKeyPath { get; set; }
    }
}