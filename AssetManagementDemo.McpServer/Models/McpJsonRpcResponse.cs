using AssetManagementDemo.McpServer.DTOs;

using System.Text.Json.Serialization;

namespace AssetManagementDemo.McpServer.Models
{
    public class McpJsonRpcResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("id")]
        public object? Id { get; set; }

        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Result { get; set; }

        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public McpJsonRpcError? Error { get; set; }

        public static McpJsonRpcResponse Success(object? id, object? result)
        {
            return new McpJsonRpcResponse
            {
                Id = id,
                Result = result
            };
        }

        public static McpJsonRpcResponse Failure(object? id, int code, string message, object? data = null)
        {
            return new McpJsonRpcResponse
            {
                Id = id,
                Error = McpJsonRpcError.Create(code, message, data)
            };
        }
    }
}
