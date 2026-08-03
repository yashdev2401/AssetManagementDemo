using System.Threading.Tasks;
using AssetManagementDemo.Web.Data;
using AssetManagementDemo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementDemo.Web.Repositories
{
    public class AssetRepository : GenericRepository<Asset>, IAssetRepository
    {
        public AssetRepository(AssetDbContext context) : base(context)
        {
        }

        public async Task<Asset?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AssetCode == code);
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeAssetId = null)
        {
            if (excludeAssetId.HasValue)
            {
                return !await _dbSet.AnyAsync(a => a.AssetCode == code && a.AssetId != excludeAssetId.Value);
            }
            return !await _dbSet.AnyAsync(a => a.AssetCode == code);
        }

        public async Task<bool> IsSerialNumberUniqueAsync(string serialNumber, int? excludeAssetId = null)
        {
            if (string.IsNullOrWhiteSpace(serialNumber)) return true;

            if (excludeAssetId.HasValue)
            {
                return !await _dbSet.AnyAsync(a => a.SerialNumber == serialNumber && a.AssetId != excludeAssetId.Value);
            }
            return !await _dbSet.AnyAsync(a => a.SerialNumber == serialNumber);
        }
    }
}
