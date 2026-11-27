using QLNhanSu.DTOs;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repo;

        public LeaveRequestService(ILeaveRequestRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<LeaveRequestDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(ToDto).ToList();
        }

        public async Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto)
        {
            if (!await _repo.EmployeeExistsAsync(dto.EmployeeId))
                throw new BadRequestException("EmployeeId không tồn tại.");

            if (dto.EndDate < dto.StartDate)
                throw new BadRequestException("Ngày kết thúc phải sau ngày bắt đầu.");

            var leave = new LeaveRequest
            {
                EmployeeId = dto.EmployeeId,
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
            var leave = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy đơn nghỉ phép có Id = {id}.");

            if (dto.Status != "Approved" && dto.Status != "Rejected")
                throw new BadRequestException("Status chỉ được là 'Approved' hoặc 'Rejected'.");

            leave.Status = dto.Status;
            await _repo.SaveChangesAsync();
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
