// Đặt tại: Repositories/IDepartmentRepository.cs
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> HasEmployeesAsync(int id);
        Task AddAsync(Department department);
        void Remove(Department department);
        Task<bool> SaveChangesAsync();
    }
}