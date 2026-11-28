using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IAttendanceRepository
    {
        Task<AttendanceRecord?> GetByEmployeeAndDateAsync(
            int employeeId,
            DateTime workDate);

        Task<List<AttendanceRecord>> GetMonthAsync(
            DateTime startDate,
            DateTime endDate,
            int? employeeId = null);

        Task AddAsync(AttendanceRecord record);

        Task<bool> SaveChangesAsync();
    }
}
