using QLNhanSu.DTOs.Common;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IContractRepository
    {
        Task<PagedResult<Contract>> GetAllAsync(PaginationParameters page, int? employeeId = null);
        Task<Contract?> GetByIdAsync(int id);
        Task<bool> EmployeeExistsAsync(int employeeId);
        Task AddAsync(Contract contract);
        Task<bool> SaveChangesAsync();
    }
}
