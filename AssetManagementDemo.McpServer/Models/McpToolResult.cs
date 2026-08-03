using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AssetManagementDemo.McpServer.Models
{
    public class McpContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class McpToolResult
    {
        [JsonPropertyName("content")]
        public List<McpContentItem> Content { get; set; } = new List<McpContentItem>();

        [JsonPropertyName("isError")]
        public bool IsError { get; set; } = false;

        public static McpToolResult Success(string text)
        {
            return new McpToolResult
            {
                Content = new List<McpContentItem>
                {
                    new McpContentItem { Type = "text", Text = text }
                },
                IsError = false
            };
        }

        public static McpToolResult Error(string errorText)
        {
            return new McpToolResult
            {
                Content = new List<McpContentItem>
                {
                    new McpContentItem { Type = "text", Text = errorText }
                },
                IsError = true
            };
        }
    }
}
