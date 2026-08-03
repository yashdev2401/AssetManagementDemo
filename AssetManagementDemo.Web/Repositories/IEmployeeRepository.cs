using System.Threading.Tasks;
using AssetManagementDemo.Web.Models;

namespace AssetManagementDemo.Web.Repositories
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetByCodeAsync(string code);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeEmployeeId = null);
    }
}
