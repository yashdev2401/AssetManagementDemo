using System.Collections.Generic;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.ViewModels;

namespace AssetManagementDemo.Web.Services
{
    public interface IAssetService
    {
        Task<PagedResult<Asset>> GetAssetsPagedAsync(
            string? searchTerm, 
            string? category, 
            string? brand,
            string? status, 
            string? sortBy,
            bool sortDescending,
            int pageNumber, 
            int pageSize);
        Task<IEnumerable<Asset>> GetAllAssetsAsync();
        Task<IEnumerable<Asset>> GetAvailableAssetsAsync();
        Task<Asset?> GetAssetByIdAsync(int id);
        Task<Asset?> GetAssetByCodeAsync(string code);
        Task<Asset> CreateAssetAsync(Asset asset);
        Task<bool> UpdateAssetAsync(Asset asset);
        Task<bool> DeleteAssetAsync(int id);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<bool> IsSerialNumberUniqueAsync(string serialNumber, int? excludeId = null);
        AssetDto MapToDto(Asset asset);
        Asset MapToEntity(CreateAssetDto dto);
    }
}
