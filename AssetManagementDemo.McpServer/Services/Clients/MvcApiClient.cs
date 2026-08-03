using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using AssetManagementDemo.McpServer.Configuration;
using Microsoft.Extensions.Options;
using AssetManagementDemo.McpServer.DTOs;
using AssetManagementDemo.McpServer.Helpers;
using AssetManagementDemo.McpServer.Services.Interfaces;

namespace AssetManagementDemo.McpServer.Services.Clients
{
    public class MvcApiClient : IMvcApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MvcApiClient> _logger;
		private readonly MvcApiOptions _options;

		public MvcApiClient(HttpClient httpClient, ILogger<MvcApiClient> logger, IOptions<MvcApiOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<bool> CheckHealthAsync(string? correlationId = null, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();
            var cid = correlationId ?? Guid.NewGuid().ToString("N");
            _logger.LogInformation("[CorrelationId: {CorrelationId}] Health Check -> Testing REST API connectivity at {BaseUrl}", cid, _httpClient.BaseAddress);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/EmployeesApi?pageSize=1");
                request.Headers.Add("X-Correlation-ID", cid);
                request.Headers.Add("X-API-Key", _options.ApiKey);

                var response = await _httpClient.SendAsync(request, ct);
                sw.Stop();
                _logger.LogInformation("[CorrelationId: {CorrelationId}] Health Check Result: Status {Status} (Elapsed: {ElapsedMs}ms)", cid, response.StatusCode, sw.ElapsedMilliseconds);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[CorrelationId: {CorrelationId}] Health Check Failed (Elapsed: {ElapsedMs}ms)", cid, sw.ElapsedMilliseconds);
                return false;
            }
        }

        #region Employees API

        public async Task<ApiResponseDto<PagedResultDto<EmployeeDto>>?> GetEmployeesAsync(
            string? searchTerm, string? department, string? status, string? location, 
            string? sortBy, bool sortDescending, int pageNumber, int pageSize, string? correlationId = null, CancellationToken ct = default)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(searchTerm)) query["searchTerm"] = searchTerm;
            if (!string.IsNullOrWhiteSpace(department)) query["department"] = department;
            if (!string.IsNullOrWhiteSpace(status)) query["status"] = status;
            if (!string.IsNullOrWhiteSpace(location)) query["location"] = location;
            if (!string.IsNullOrWhiteSpace(sortBy)) query["sortBy"] = sortBy;
            query["sortDescending"] = sortDescending.ToString().ToLower();
            query["pageNumber"] = pageNumber.ToString();
            query["pageSize"] = pageSize.ToString();

            var endpoint = $"api/EmployeesApi?{query}";
            return await ExecuteRequestAsync<ApiResponseDto<PagedResultDto<EmployeeDto>>>(HttpMethod.Get, endpoint, null, correlationId, ct);
        }

        public async Task<ApiResponseDto<EmployeeDto>?> GetEmployeeByIdAsync(int id, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<EmployeeDto>>(HttpMethod.Get, $"api/EmployeesApi/{id}", null, correlationId, ct);
        }

        public async Task<ApiResponseDto<EmployeeDto>?> CreateEmployeeAsync(CreateEmployeeDto dto, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<EmployeeDto>>(HttpMethod.Post, "api/EmployeesApi", dto, correlationId, ct);
        }

        public async Task<ApiResponseDto<EmployeeDto>?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<EmployeeDto>>(HttpMethod.Put, $"api/EmployeesApi/{id}", dto, correlationId, ct);
        }

        public async Task<ApiResponseDto<bool>?> DeleteEmployeeAsync(int id, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<bool>>(HttpMethod.Delete, $"api/EmployeesApi/{id}", null, correlationId, ct);
        }

        #endregion

        #region Assets API

        public async Task<ApiResponseDto<PagedResultDto<AssetDto>>?> GetAssetsAsync(
            string? searchTerm, string? category, string? brand, string? status, 
            string? sortBy, bool sortDescending, int pageNumber, int pageSize, string? correlationId = null, CancellationToken ct = default)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(searchTerm)) query["searchTerm"] = searchTerm;
            if (!string.IsNullOrWhiteSpace(category)) query["category"] = category;
            if (!string.IsNullOrWhiteSpace(brand)) query["brand"] = brand;
            if (!string.IsNullOrWhiteSpace(status)) query["status"] = status;
            if (!string.IsNullOrWhiteSpace(sortBy)) query["sortBy"] = sortBy;
            query["sortDescending"] = sortDescending.ToString().ToLower();
            query["pageNumber"] = pageNumber.ToString();
            query["pageSize"] = pageSize.ToString();

            var endpoint = $"api/AssetsApi?{query}";
            return await ExecuteRequestAsync<ApiResponseDto<PagedResultDto<AssetDto>>>(HttpMethod.Get, endpoint, null, correlationId, ct);
        }

        public async Task<ApiResponseDto<AssetDto>?> GetAssetByIdAsync(int id, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<AssetDto>>(HttpMethod.Get, $"api/AssetsApi/{id}", null, correlationId, ct);
        }

        public async Task<ApiResponseDto<AssetDto>?> CreateAssetAsync(CreateAssetDto dto, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<AssetDto>>(HttpMethod.Post, "api/AssetsApi", dto, correlationId, ct);
        }

        public async Task<ApiResponseDto<AssetDto>?> UpdateAssetAsync(int id, UpdateAssetDto dto, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<AssetDto>>(HttpMethod.Put, $"api/AssetsApi/{id}", dto, correlationId, ct);
        }

        public async Task<ApiResponseDto<bool>?> DeleteAssetAsync(int id, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<bool>>(HttpMethod.Delete, $"api/AssetsApi/{id}", null, correlationId, ct);
        }

        #endregion

        #region Assignments API

        public async Task<ApiResponseDto<PagedResultDto<AssignmentDto>>?> GetAssignmentsAsync(
            string? searchTerm, bool? isActive, string? assignedDate, 
            string? sortBy, bool sortDescending, int pageNumber, int pageSize, string? correlationId = null, CancellationToken ct = default)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(searchTerm)) query["searchTerm"] = searchTerm;
            if (isActive.HasValue) query["isActive"] = isActive.Value.ToString().ToLower();
            if (!string.IsNullOrWhiteSpace(assignedDate)) query["assignedDate"] = assignedDate;
            if (!string.IsNullOrWhiteSpace(sortBy)) query["sortBy"] = sortBy;
            query["sortDescending"] = sortDescending.ToString().ToLower();
            query["pageNumber"] = pageNumber.ToString();
            query["pageSize"] = pageSize.ToString();

            var endpoint = $"api/AssignmentsApi?{query}";
            return await ExecuteRequestAsync<ApiResponseDto<PagedResultDto<AssignmentDto>>>(HttpMethod.Get, endpoint, null, correlationId, ct);
        }

        public async Task<ApiResponseDto<AssignmentDto>?> GetAssignmentByIdAsync(int id, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<AssignmentDto>>(HttpMethod.Get, $"api/AssignmentsApi/{id}", null, correlationId, ct);
        }

        public async Task<ApiResponseDto<AssignmentDto>?> AssignAssetAsync(AssignAssetDto dto, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<AssignmentDto>>(HttpMethod.Post, "api/AssignmentsApi/assign", dto, correlationId, ct);
        }

        public async Task<ApiResponseDto<bool>?> ReturnAssetAsync(ReturnAssetDto dto, string? correlationId = null, CancellationToken ct = default)
        {
            return await ExecuteRequestAsync<ApiResponseDto<bool>>(HttpMethod.Post, "api/AssignmentsApi/return", dto, correlationId, ct);
        }

        #endregion

        #region Generic HTTP Pipeline Execution & Response Translation

        private async Task<TResult?> ExecuteRequestAsync<TResult>(HttpMethod method, string endpoint, object? body = null, string? correlationId = null, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();
            var cid = correlationId ?? Guid.NewGuid().ToString("N");

            _logger.LogInformation("[CorrelationId: {CorrelationId}] HTTP {Method} -> {Endpoint}", cid, method.Method, endpoint);

            try
            {
                using var request = new HttpRequestMessage(method, endpoint);
                request.Headers.Add("X-Correlation-ID", cid);
				request.Headers.Add("X-API-Key", _options.ApiKey);

				if (body != null && (method == HttpMethod.Post || method == HttpMethod.Put))
                {
                    request.Content = JsonContent.Create(body, options: JsonSerializationHelper.DefaultOptions);
                }

                var response = await _httpClient.SendAsync(request, ct);
                sw.Stop();

                var rawResponseBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogInformation("[CorrelationId: {CorrelationId}] HTTP {Method} <- {Endpoint} Status: {Status} (Elapsed: {ElapsedMs}ms)", 
                    cid, method.Method, endpoint, (int)response.StatusCode, sw.ElapsedMilliseconds);

                if (!string.IsNullOrWhiteSpace(rawResponseBody))
                {
                    try
                    {
                        var deserialized = JsonSerializationHelper.Deserialize<TResult>(rawResponseBody);
                        if (deserialized != null) return deserialized;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[CorrelationId: {CorrelationId}] Deserialization warning on response body: {ResponseBody}", cid, rawResponseBody);
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[CorrelationId: {CorrelationId}] HTTP {Method} <- {Endpoint} Non-Success Status Code {StatusCode}: {ResponseBody}", 
                        cid, method.Method, endpoint, (int)response.StatusCode, rawResponseBody);
                }

                return default;
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                sw.Stop();
                _logger.LogError(ex, "[CorrelationId: {CorrelationId}] HTTP Request Timeout -> {Endpoint} (Elapsed: {ElapsedMs}ms)", cid, endpoint, sw.ElapsedMilliseconds);
                throw new TimeoutException($"REST API request timed out after {sw.ElapsedMilliseconds}ms.", ex);
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[CorrelationId: {CorrelationId}] HTTP Connection Refused / Network Error -> {Endpoint} (Elapsed: {ElapsedMs}ms)", cid, endpoint, sw.ElapsedMilliseconds);
                throw new InvalidOperationException($"REST API connection failed to '{endpoint}'. Ensure MVC Application is running.", ex);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[CorrelationId: {CorrelationId}] HTTP {Method} failed -> {Endpoint} (Elapsed: {ElapsedMs}ms)", cid, method.Method, endpoint, sw.ElapsedMilliseconds);
                throw;
            }
        }

        #endregion
    }
}
