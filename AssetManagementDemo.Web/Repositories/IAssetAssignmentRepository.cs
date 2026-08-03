using System.Collections.Generic;
using System.Threading.Tasks;
using AssetManagementDemo.Web.Models;

namespace AssetManagementDemo.Web.Repositories
{
    public interface IAssetAssignmentRepository : IGenericRepository<AssetAssignment>
    {
        Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId);
        Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<AssetAssignment>> GetAssignmentsByAssetIdAsync(int assetId);
        Task<AssetAssignment?> GetWithDetailsByIdAsync(int assignmentId);
    }
}
