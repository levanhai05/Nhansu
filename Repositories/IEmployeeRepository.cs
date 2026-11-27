// Đặt tại: Repositories/IEmployeeRepository.cs
using QLNhanSu.DTOs.Common;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IEmployeeRepository
    {
        Task<PagedResult<Employee>> GetPagedAsync(EmployeeQueryParameters queryParams);
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        Task<bool> DepartmentExistsAsync(int departmentId);
        Task<bool> PositionExistsAsync(int positionId);
        Task<bool> SaveChangesAsync();
        Task<int?> GetActiveEmployeeIdByUserIdAsync(int userId);
        Task<bool> IsUserLinkedAsync(int userId);
        Task<bool> ChangeUserLinkAsync(int employeeId, int? expectedUserId, int? newUserId);
    }
}