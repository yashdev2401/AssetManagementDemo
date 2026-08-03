using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Helpers;
using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.Repositories;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.Web.Services
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly ILogger<AssetService> _logger;

        public AssetService(IAssetRepository assetRepository, ILogger<AssetService> logger)
        {
            _assetRepository = assetRepository;
            _logger = logger;
        }

        public async Task<PagedResult<Asset>> GetAssetsPagedAsync(
            string? searchTerm, 
            string? category, 
            string? brand,
            string? status, 
            string? sortBy,
            bool sortDescending,
            int pageNumber, 
            int pageSize)
        {
            _logger.LogInformation("Querying assets. Search: {SearchTerm}, Cat: {Category}, Brand: {Brand}, Status: {Status}, SortBy: {SortBy}, SortDesc: {SortDesc}, Page: {PageNumber}, Size: {PageSize}",
                searchTerm, category, brand, status, sortBy, sortDescending, pageNumber, pageSize);

            var query = _assetRepository.GetQueryable(tracking: false);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(a => a.AssetCode.ToLower().Contains(term) ||
                                         a.AssetName.ToLower().Contains(term) ||
                                         (a.Category != null && a.Category.ToLower().Contains(term)) ||
                                         (a.Brand != null && a.Brand.ToLower().Contains(term)) ||
                                         (a.Model != null && a.Model.ToLower().Contains(term)) ||
                                         (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(term)) ||
                                         a.Status.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(a => a.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(a => a.Brand == brand);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            query = sortBy?.ToLower() switch
            {
                "assetcode" or "code" => sortDescending ? query.OrderByDescending(a => a.AssetCode) : query.OrderBy(a => a.AssetCode),
                "assetname" or "name" => sortDescending ? query.OrderByDescending(a => a.AssetName) : query.OrderBy(a => a.AssetName),
                "category" => sortDescending ? query.OrderByDescending(a => a.Category) : query.OrderBy(a => a.Category),
                "brand" => sortDescending ? query.OrderByDescending(a => a.Brand) : query.OrderBy(a => a.Brand),
                "model" => sortDescending ? query.OrderByDescending(a => a.Model) : query.OrderBy(a => a.Model),
                "purchasedate" => sortDescending ? query.OrderByDescending(a => a.PurchaseDate) : query.OrderBy(a => a.PurchaseDate),
                "purchaseprice" or "price" => sortDescending ? query.OrderByDescending(a => a.PurchasePrice) : query.OrderBy(a => a.PurchasePrice),
                "status" => sortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                _ => sortDescending ? query.OrderByDescending(a => a.AssetId) : query.OrderByDescending(a => a.AssetId)
            };

            return await query.ToPagedResultAsync(pageNumber, pageSize, searchTerm, sortBy, sortDescending);
        }

        public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
        {
            return await _assetRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Asset>> GetAvailableAssetsAsync()
        {
            return await _assetRepository.FindAsync(a => a.Status == "Available");
        }

        public async Task<Asset?> GetAssetByIdAsync(int id)
        {
            return await _assetRepository.GetByIdAsync(id);
        }

        public async Task<Asset?> GetAssetByCodeAsync(string code)
        {
            return await _assetRepository.GetByCodeAsync(code);
        }

        public async Task<Asset> CreateAssetAsync(Asset asset)
        {
            _logger.LogInformation("Creating asset with Code: {AssetCode}", asset.AssetCode);

            if (!await _assetRepository.IsCodeUniqueAsync(asset.AssetCode))
            {
                _logger.LogWarning("Asset creation failed. Code '{AssetCode}' already exists", asset.AssetCode);
                throw new InvalidOperationException($"Asset Code '{asset.AssetCode}' is already in use.");
            }

            if (!string.IsNullOrWhiteSpace(asset.SerialNumber) && !await _assetRepository.IsSerialNumberUniqueAsync(asset.SerialNumber))
            {
                _logger.LogWarning("Asset creation failed. Serial Number '{SerialNumber}' already exists", asset.SerialNumber);
                throw new InvalidOperationException($"Serial Number '{asset.SerialNumber}' is already in use.");
            }

            asset.CreatedDate = DateTime.Now;
            await _assetRepository.AddAsync(asset);
            await _assetRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully created asset ID {AssetId} ({AssetCode} - {AssetName})", asset.AssetId, asset.AssetCode, asset.AssetName);
            return asset;
        }

        public async Task<bool> UpdateAssetAsync(Asset asset)
        {
            _logger.LogInformation("Updating asset ID {AssetId}", asset.AssetId);

            var existing = await _assetRepository.GetByIdAsync(asset.AssetId);
            if (existing == null)
            {
                _logger.LogWarning("Asset update failed. ID {AssetId} not found", asset.AssetId);
                return false;
            }

            if (!await _assetRepository.IsCodeUniqueAsync(asset.AssetCode, asset.AssetId))
            {
                _logger.LogWarning("Asset update failed. Code '{AssetCode}' already exists for another asset", asset.AssetCode);
                throw new InvalidOperationException($"Asset Code '{asset.AssetCode}' is already in use.");
            }

            if (!string.IsNullOrWhiteSpace(asset.SerialNumber) && !await _assetRepository.IsSerialNumberUniqueAsync(asset.SerialNumber, asset.AssetId))
            {
                _logger.LogWarning("Asset update failed. Serial Number '{SerialNumber}' already exists for another asset", asset.SerialNumber);
                throw new InvalidOperationException($"Serial Number '{asset.SerialNumber}' is already in use.");
            }

            existing.AssetCode = asset.AssetCode;
            existing.AssetName = asset.AssetName;
            existing.Category = asset.Category;
            existing.Brand = asset.Brand;
            existing.Model = asset.Model;
            existing.SerialNumber = asset.SerialNumber;
            existing.PurchaseDate = asset.PurchaseDate;
            existing.WarrantyExpiry = asset.WarrantyExpiry;
            existing.PurchasePrice = asset.PurchasePrice;
            existing.Status = asset.Status;
            existing.UpdatedDate = DateTime.Now;

            _assetRepository.Update(existing);
            await _assetRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully updated asset ID {AssetId}", asset.AssetId);
            return true;
        }

        public async Task<bool> DeleteAssetAsync(int id)
        {
            _logger.LogInformation("Deleting asset ID {AssetId}", id);

            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null)
            {
                _logger.LogWarning("Asset deletion failed. ID {AssetId} not found", id);
                return false;
            }

            _assetRepository.Remove(asset);
            await _assetRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted asset ID {AssetId}", id);
            return true;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            return await _assetRepository.IsCodeUniqueAsync(code, excludeId);
        }

        public async Task<bool> IsSerialNumberUniqueAsync(string serialNumber, int? excludeId = null)
        {
            return await _assetRepository.IsSerialNumberUniqueAsync(serialNumber, excludeId);
        }

        public AssetDto MapToDto(Asset asset)
        {
            return new AssetDto
            {
                AssetId = asset.AssetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                Category = asset.Category,
                Brand = asset.Brand,
                Model = asset.Model,
                SerialNumber = asset.SerialNumber,
                PurchaseDate = asset.PurchaseDate,
                WarrantyExpiry = asset.WarrantyExpiry,
                PurchasePrice = asset.PurchasePrice,
                Status = asset.Status,
                CreatedDate = asset.CreatedDate,
                UpdatedDate = asset.UpdatedDate
            };
        }

        public Asset MapToEntity(CreateAssetDto dto)
        {
            return new Asset
            {
                AssetCode = dto.AssetCode,
                AssetName = dto.AssetName,
                Category = dto.Category,
                Brand = dto.Brand,
                Model = dto.Model,
                SerialNumber = dto.SerialNumber,
                PurchaseDate = dto.PurchaseDate,
                WarrantyExpiry = dto.WarrantyExpiry,
                PurchasePrice = dto.PurchasePrice,
                Status = dto.Status ?? "Available"
            };
        }
    }
}
