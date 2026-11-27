using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IContractRepository
    {
        Task<List<Contract>> GetAllAsync();
        Task<List<Contract>> GetByEmployeeIdAsync(int employeeId);
        Task<Contract?> GetByIdAsync(int id);
        Task<bool> EmployeeExistsAsync(int employeeId);
        Task AddAsync(Contract contract);
        Task<bool> SaveChangesAsync();
    }
}
