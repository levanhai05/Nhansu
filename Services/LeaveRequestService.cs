using QLNhanSu.DTOs;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repo;
        private readonly CurrentUserService _current;

        public LeaveRequestService(ILeaveRequestRepository repo, CurrentUserService current)
        {
            _repo = repo;
            _current = current;
        }

        public async Task<List<LeaveRequestDto>> GetAllAsync()
        {
            var actor = await _current.GetAsync();
            int? employeeId = actor.Role == "User" ? await _current.EmployeeIdAsync(actor.Id) : null;
            var items = await _repo.GetAllAsync(employeeId);
            return items.Select(ToDto).ToList();
        }

        public async Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto)
        {
            var actor = await _current.GetAsync();
            var employeeId = await _current.EmployeeIdAsync(actor.Id);
            if (dto.EmployeeId != employeeId)
                throw new AppException("Chỉ được gửi đơn cho chính mình.", 403);
            if (!await _repo.EmployeeExistsAsync(employeeId))
                throw new BadRequestException("EmployeeId không tồn tại.");

            if (dto.EndDate < dto.StartDate)
                throw new BadRequestException("Ngày kết thúc phải sau ngày bắt đầu.");

            var leave = new LeaveRequest
            {
                EmployeeId = employeeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(leave);
            await _repo.SaveChangesAsync();

            // Nạp lại để có tên nhân viên trong DTO trả về
            var created = await _repo.GetByIdAsync(leave.Id);
            return ToDto(created!);
        }

        public async Task UpdateStatusAsync(int id, LeaveRequestUpdateStatusDto dto)
        {
            var actor = await _current.GetAsync();
            if (actor.Role is not ("Admin" or "HR")) throw new AppException("Không đủ quyền.", 403);
            var leave = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy đơn nghỉ phép có Id = {id}.");

            if (dto.Status != "Approved" && dto.Status != "Rejected")
                throw new BadRequestException("Status chỉ được là 'Approved' hoặc 'Rejected'.");

            if (leave.Status != "Pending")
                throw new AppException("Chỉ được xử lý đơn đang chờ duyệt.", 409);

            if (!await _repo.TryResolvePendingAsync(id, dto.Status))
                throw new AppException("Đơn nghỉ phép đã được xử lý hoặc không còn tồn tại.", 409);
        }

        private static LeaveRequestDto ToDto(LeaveRequest l) => new()
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeName = l.Employee?.FullName,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Reason = l.Reason,
            Status = l.Status,
            CreatedAt = l.CreatedAt
        };
    }
}
