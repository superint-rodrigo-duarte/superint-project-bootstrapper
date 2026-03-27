namespace superint.ProjectBootstrapper.DTO.Configuration
{
    public sealed class ServersSettings
    {
        public ServerSettings Stg { get; set; } = new();
        public ServerSettings Prd { get; set; } = new();
    }
}