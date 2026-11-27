using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.DTOs;
using QLNhanSu.Models;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý Đơn xin nghỉ phép của nhân viên.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaveRequestsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách toàn bộ đơn xin nghỉ phép (kèm tên nhân viên).</summary>
        // GET /api/leaverequests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Select(l => ToDto(l))
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>Nhân viên gửi đơn xin nghỉ phép mới. Trạng thái mặc định là "Pending".</summary>
        /// <param name="dto">Thông tin đơn xin nghỉ phép.</param>
        /// <response code="200">Gửi đơn thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ, EmployeeId không tồn tại, hoặc EndDate trước StartDate.</response>
        // POST /api/leaverequests  -> nhân viên gửi đơn xin nghỉ
        [HttpPost]
        public async Task<IActionResult> Create(LeaveRequestCreateDto dto)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId);
            if (!employeeExists) throw new InvalidOperationException("EmployeeId không tồn tại.");

            if (dto.EndDate < dto.StartDate)
                throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");

            var leave = new LeaveRequest
            {
                EmployeeId = dto.EmployeeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();
            await _context.Entry(leave).Reference(l => l.Employee).LoadAsync();

            return Ok(ToDto(leave));
        }

        /// <summary>HR duyệt hoặc từ chối một đơn xin nghỉ phép.</summary>
        /// <param name="id">Id của đơn xin nghỉ phép.</param>
        /// <param name="dto">Trạng thái mới ("Approved" hoặc "Rejected").</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy đơn.</response>
        /// <response code="400">Status không phải "Approved" hoặc "Rejected".</response>
        // PUT /api/leaverequests/1/status  -> HR duyệt / từ chối đơn
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, LeaveRequestUpdateStatusDto dto)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) throw new KeyNotFoundException($"Không tìm thấy đơn nghỉ phép có Id = {id}.");

            if (dto.Status != "Approved" && dto.Status != "Rejected")
                throw new InvalidOperationException("Status chỉ được là 'Approved' hoặc 'Rejected'.");

            leave.Status = dto.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static LeaveRequestDto ToDto(LeaveRequest l) => new LeaveRequestDto
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
