using System.Text.Json;

namespace superint.ProjectBootstrapper.Shared.Helpers
{
    public static class JsonSerializationHelper
    {
        public static JsonSerializerOptions DefaultApiOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static JsonSerializerOptions SnakeCaseApiOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public static JsonSerializerOptions IndentedOptions { get; } = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }
}