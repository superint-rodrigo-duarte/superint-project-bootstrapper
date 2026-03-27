using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace superint.ProjectBootstrapper.DTO
{
    public sealed class DockerEnvironmentConfigurationJson
    {
        [JsonPropertyName("deploy")]
        public bool Deploy { get; set; } = true;
        [JsonPropertyName("ReverseProxyHost")]
        public string? ReverseProxyHost { get; set; }
        [JsonPropertyName("ReverseProxyPathPrefix")]
        public string? ReverseProxyPathPrefix { get; set; }
        [JsonPropertyName("ReverseProxyPathPrefixApi")]
        public string? ReverseProxyPathPrefixApi { get; set; }
        [JsonPropertyName("localDeployPath")]
        public string? LocalDeployPath { get; set; }
        [JsonPropertyName("copyLocal")]
        public bool CopyLocal { get; set; } = false;
        [JsonPropertyName("useStripPrefixFrontend")]
        public bool UseStripPrefixFrontend { get; set; } = false;
        [JsonPropertyName("useStripPrefixBackend")]
        public bool UseStripPrefixBackend { get; set; } = true;
    }
}