using QLNhanSu.DTOs;

namespace QLNhanSu.Services
{
    public interface ILeaveRequestService
    {
        Task<List<LeaveRequestDto>> GetAllAsync();
        Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto);
        Task UpdateStatusAsync(int id, LeaveRequestUpdateStatusDto dto);
    }
}
