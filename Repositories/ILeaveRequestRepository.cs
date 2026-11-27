using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface ILeaveRequestRepository
    {
        Task<List<LeaveRequest>> GetAllAsync(int? employeeId = null);
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task<bool> EmployeeExistsAsync(int employeeId);
        Task AddAsync(LeaveRequest leaveRequest);
        Task<bool> TryResolvePendingAsync(int id, string status);
        Task<bool> SaveChangesAsync();
    }
}
