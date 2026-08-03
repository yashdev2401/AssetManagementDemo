using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.DTOs;
using AssetManagementDemo.McpServer.Helpers;
using AssetManagementDemo.McpServer.Models;
using AssetManagementDemo.McpServer.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.McpServer.Tools
{
    public class AssetTools : IMcpTool
    {
        private readonly IMvcApiClient _apiClient;
        private readonly ILogger<AssetTools> _logger;

        public AssetTools(IMvcApiClient apiClient, ILogger<AssetTools> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public IEnumerable<McpToolDefinition> GetToolDefinitions()
        {
            return new[]
            {
                new McpToolDefinition
                {
                    Name = "get_assets",
                    Description = "Returns a list of assets. Call this tool even when the user simply asks 'Show all assets'. If no filter parameters are supplied, return all assets using the default pagination. Search, category, brand, status, sorting, and pagination are all optional.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            searchTerm = new { type = "string", description = "Search code, name, category, brand, model, serial..." },
                            category = new { type = "string", description = "Filter by category (Laptop, Desktop, Monitor, Mobile, Peripheral)" },
                            brand = new { type = "string", description = "Filter by brand (Dell, HP, Lenovo, Apple, Samsung)" },
                            status = new { type = "string", description = "Filter by status (Available, Assigned, Under Repair, Retired)" },
                            sortBy = new { type = "string", description = "Column to sort by (AssetCode, AssetName, Category, Brand, PurchasePrice, Status)" },
                            sortDescending = new { type = "boolean", description = "Sort descending flag" },
                            pageNumber = new { type = "integer", description = "Page index (default 1)" },
                            pageSize = new { type = "integer", description = "Items per page (10, 20, 50, 100)" }
                        }
                    }
                },
                new McpToolDefinition
                {
                    Name = "get_asset_by_id",
                    Description = "Retrieves a single asset record by Asset ID.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new { type = "integer", description = "Target Asset ID" }
                        },
                        required = new[] { "id" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "search_assets",
                    Description = "Searches for assets matching a query string across asset code, name, category, brand, model, serial number, and status.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            query = new { type = "string", description = "Search query string" }
                        },
                        required = new[] { "query" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "create_asset",
                    Description = "Creates a new asset record in the inventory.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            assetCode = new { type = "string", description = "Unique asset code (e.g. AST001)" },
                            assetName = new { type = "string", description = "Asset name" },
                            category = new { type = "string", description = "Category" },
                            brand = new { type = "string", description = "Brand" },
                            model = new { type = "string", description = "Model" },
                            serialNumber = new { type = "string", description = "Serial number" },
                            purchaseDate = new { type = "string", description = "Purchase date (YYYY-MM-DD)" },
                            warrantyExpiry = new { type = "string", description = "Warranty expiry date (YYYY-MM-DD)" },
                            purchasePrice = new { type = "number", description = "Purchase price" },
                            status = new { type = "string", description = "Status (Available, Assigned, Under Repair, Retired)" }
                        },

                        required = new[] { "assetCode", "assetName" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "update_asset",
                    Description = "Updates an existing asset record by Asset ID.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new { type = "integer", description = "Asset ID to update" },
                            assetCode = new { type = "string", description = "Asset code" },
                            assetName = new { type = "string", description = "Asset name" },
                            category = new { type = "string", description = "Category" },
                            brand = new { type = "string", description = "Brand" },
                            model = new { type = "string", description = "Model" },
                            serialNumber = new { type = "string", description = "Serial number" },
                            purchaseDate = new { type = "string", description = "Purchase date" },
                            warrantyExpiry = new { type = "string", description = "Warranty expiry" },
                            purchasePrice = new { type = "number", description = "Purchase price" },
                            status = new { type = "string", description = "Status" }
                        },
                        required = new[] {"id"}
                    }
                },
                new McpToolDefinition
                {
                    Name = "delete_asset",
                    Description = "Deletes an asset record by Asset ID.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new { type = "integer", description = "Asset ID to delete" }
                        },
                        required = new[] { "id" }
                    }
                }
            };
        }

        public async Task<McpToolResult> ExecuteToolAsync(string toolName, JsonElement? args, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing AssetTool: {ToolName}", toolName);

            try
            {
                return toolName.ToLower() switch
                {
                    "get_assets" => await ExecuteGetAssetsAsync(args, ct),
                    "get_asset_by_id" => await ExecuteGetAssetByIdAsync(args, ct),
                    "search_assets" => await ExecuteSearchAssetsAsync(args, ct),
                    "create_asset" => await ExecuteCreateAssetAsync(args, ct),
                    "update_asset" => await ExecuteUpdateAssetAsync(args, ct),
                    "delete_asset" => await ExecuteDeleteAssetAsync(args, ct),
                    _ => McpToolResult.Error($"Unknown asset tool '{toolName}'")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing AssetTool {ToolName}", toolName);
                return McpToolResult.Error($"Execution error: {ex.Message}");
            }
        }

        private async Task<McpToolResult> ExecuteGetAssetsAsync(JsonElement? args, CancellationToken ct)
        {
            var searchTerm = GetStringProp(args, "searchTerm");
            var category = GetStringProp(args, "category");
            var brand = GetStringProp(args, "brand");
            var status = GetStringProp(args, "status");
            var sortBy = GetStringProp(args, "sortBy");
            var sortDesc = GetBoolProp(args, "sortDescending") ?? false;
            var pageNumber = GetIntProp(args, "pageNumber") ?? 1;
            var pageSize = GetIntProp(args, "pageSize") ?? 10;

            var response = await _apiClient.GetAssetsAsync(searchTerm, category, brand, status, sortBy, sortDesc, pageNumber, pageSize, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteGetAssetByIdAsync(JsonElement? args, CancellationToken ct)
        {
            var id = GetIntProp(args, "id");
            if (!id.HasValue) return McpToolResult.Error("Parameter 'id' is required");

            var response = await _apiClient.GetAssetByIdAsync(id.Value, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteSearchAssetsAsync(JsonElement? args, CancellationToken ct)
        {
            var query = GetStringProp(args, "query") ?? GetStringProp(args, "searchTerm");
            if (string.IsNullOrWhiteSpace(query)) return McpToolResult.Error("Parameter 'query' is required");

            var response = await _apiClient.GetAssetsAsync(query, null, null, null, null, false, 1, 20, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteCreateAssetAsync(JsonElement? args, CancellationToken ct)
        {
            var dto = new CreateAssetDto
            {
                AssetCode = GetStringProp(args, "assetCode") ?? string.Empty,
                AssetName = GetStringProp(args, "assetName") ?? string.Empty,
                Category = GetStringProp(args, "category"),
                Brand = GetStringProp(args, "brand"),
                Model = GetStringProp(args, "model"),
                SerialNumber = GetStringProp(args, "serialNumber"),
                PurchaseDate = GetDateTimeProp(args, "purchaseDate"),
                WarrantyExpiry = GetDateTimeProp(args, "warrantyExpiry"),
                PurchasePrice = GetDecimalProp(args, "purchasePrice"),
                Status = GetStringProp(args, "status") ?? "Available"
            };

            if (string.IsNullOrWhiteSpace(dto.AssetCode) || string.IsNullOrWhiteSpace(dto.AssetName))
            {
                return McpToolResult.Error("Parameters 'assetCode' and 'assetName' are required.");
            }

            var response = await _apiClient.CreateAssetAsync(dto, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteUpdateAssetAsync(JsonElement? args, CancellationToken ct)
        {
            var id = GetIntProp(args, "id");
            if (!id.HasValue) return McpToolResult.Error("Parameter 'id' is required");
            var existingResponse = await _apiClient.GetAssetByIdAsync(id.Value, ct: ct);
            if (existingResponse == null || !existingResponse.Success || existingResponse.Data == null)
            {
                return McpToolResult.Error($"Asset with ID {id.Value} not found.");
            }
            
            var dto = new UpdateAssetDto
            {
                AssetCode = GetStringProp(args, "assetCode"),
                AssetName = GetStringProp(args, "assetName"),
                Category = GetStringProp(args, "category"),
                Brand = GetStringProp(args, "brand"),
                Model = GetStringProp(args, "model"),
                SerialNumber = GetStringProp(args, "serialNumber"),
                PurchaseDate = GetDateTimeProp(args, "purchaseDate"),
                WarrantyExpiry = GetDateTimeProp(args, "warrantyExpiry"),
                PurchasePrice = GetDecimalProp(args, "purchasePrice"),
                Status = GetStringProp(args, "status")
            };

            var response = await _apiClient.UpdateAssetAsync(id.Value, dto, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteDeleteAssetAsync(JsonElement? args, CancellationToken ct)
        {
            var id = GetIntProp(args, "id");
            if (!id.HasValue) return McpToolResult.Error("Parameter 'id' is required");

            var response = await _apiClient.DeleteAssetAsync(id.Value, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        #region Helper Extraction Methods

        private static string? GetStringProp(JsonElement? element, string name)
        {
            if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object && element.Value.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
            return null;
        }

        private static int? GetIntProp(JsonElement? element, string name)
        {
            if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object && element.Value.TryGetProperty(name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var val)) return val;
                if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static decimal? GetDecimalProp(JsonElement? element, string name)
        {
            if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object && element.Value.TryGetProperty(name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var val)) return val;
                if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static bool? GetBoolProp(JsonElement? element, string name)
        {
            if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object && element.Value.TryGetProperty(name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.True || prop.ValueKind == JsonValueKind.False) return prop.GetBoolean();
                if (prop.ValueKind == JsonValueKind.String && bool.TryParse(prop.GetString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static DateTime? GetDateTimeProp(JsonElement? element, string name)
        {
            var str = GetStringProp(element, name);
            if (!string.IsNullOrWhiteSpace(str) && DateTime.TryParse(str, out var dt)) return dt;
            return null;
        }

        #endregion
    }
}
