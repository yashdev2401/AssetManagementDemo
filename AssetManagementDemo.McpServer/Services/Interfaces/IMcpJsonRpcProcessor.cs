using System.Threading;
using System.Threading.Tasks;

namespace AssetManagementDemo.McpServer.Services.Interfaces
{
    public interface IMcpJsonRpcProcessor
    {
        Task<string> ProcessRequestAsync(string rawJson, string? correlationId = null, CancellationToken ct = default);
    }
}
