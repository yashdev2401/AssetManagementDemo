using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AssetManagementDemo.McpServer.Models
{
    public class McpToolDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("inputSchema")]
        public object InputSchema { get; set; } = new { type = "object", properties = new { } };
    }
}
