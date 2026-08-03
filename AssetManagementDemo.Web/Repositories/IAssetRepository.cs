using System.Threading.Tasks;
using AssetManagementDemo.Web.Models;

namespace AssetManagementDemo.Web.Repositories
{
    public interface IAssetRepository : IGenericRepository<Asset>
    {
        Task<Asset?> GetByCodeAsync(string code);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeAssetId = null);
        Task<bool> IsSerialNumberUniqueAsync(string serialNumber, int? excludeAssetId = null);
    }
}
