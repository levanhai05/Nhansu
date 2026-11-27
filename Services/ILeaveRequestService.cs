using QLNhanSu.DTOs.Common;
using QLNhanSu.DTOs;

namespace QLNhanSu.Services
{
    public interface ILeaveRequestService
    {
        Task<PagedResult<LeaveRequestDto>> GetAllAsync(PaginationParameters page);
        Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto);
        Task UpdateStatusAsync(int id, LeaveRequestUpdateStatusDto dto);
    }
}
