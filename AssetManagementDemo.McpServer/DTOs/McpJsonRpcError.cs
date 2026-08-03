using System.Text.Json.Serialization;

namespace AssetManagementDemo.McpServer.DTOs
{
    public class McpJsonRpcError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        public static McpJsonRpcError Create(int code, string message, object? data = null)
        {
            return new McpJsonRpcError
            {
                Code = code,
                Message = message,
                Data = data
            };
        }
    }
}
