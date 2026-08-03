using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetManagementDemo.Web.Data;
using AssetManagementDemo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementDemo.Web.Repositories
{
    public class AssetAssignmentRepository : GenericRepository<AssetAssignment>, IAssetAssignmentRepository
    {
        public AssetAssignmentRepository(AssetDbContext context) : base(context)
        {
        }

        public async Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId)
        {
            return await _dbSet
                .Include(aa => aa.Employee)
                .Include(aa => aa.Asset)
                .FirstOrDefaultAsync(aa => aa.AssetId == assetId && aa.IsActive == true);
        }

        public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(aa => aa.Asset)
                .Where(aa => aa.EmployeeId == employeeId)
                .OrderByDescending(aa => aa.AssignedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByAssetIdAsync(int assetId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(aa => aa.Employee)
                .Where(aa => aa.AssetId == assetId)
                .OrderByDescending(aa => aa.AssignedDate)
                .ToListAsync();
        }

        public async Task<AssetAssignment?> GetWithDetailsByIdAsync(int assignmentId)
        {
            return await _dbSet
                .Include(aa => aa.Employee)
                .Include(aa => aa.Asset)
                .FirstOrDefaultAsync(aa => aa.AssignmentId == assignmentId);
        }
    }
}
