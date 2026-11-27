using QLNhanSu.DTOs.Attendance;

namespace QLNhanSu.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceDto> CheckInAsync();

        Task<AttendanceDto> CheckOutAsync();

        Task<AttendanceSummaryDto> GetMyMonthAsync(int year, int month);

        Task<AttendanceSummaryDto> GetAllMonthAsync(int year, int month);
    }
}
