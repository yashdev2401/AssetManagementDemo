using System.Threading.Tasks;
using AssetManagementDemo.Web.Data;
using AssetManagementDemo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementDemo.Web.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AssetDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeCode == code);
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeEmployeeId = null)
        {
            if (excludeEmployeeId.HasValue)
            {
                return !await _dbSet.AnyAsync(e => e.EmployeeCode == code && e.EmployeeId != excludeEmployeeId.Value);
            }
            return !await _dbSet.AnyAsync(e => e.EmployeeCode == code);
        }
    }
}
