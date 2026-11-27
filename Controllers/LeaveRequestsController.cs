using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs;
using QLNhanSu.Services;

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
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        /// <summary>Lấy danh sách toàn bộ đơn xin nghỉ phép (kèm tên nhân viên).</summary>
        // GET /api/leaverequests
        [HttpGet]
        public async Task<ActionResult<List<LeaveRequestDto>>> GetAll()
        {
            return Ok(await _leaveRequestService.GetAllAsync());
        }

        /// <summary>Nhân viên gửi đơn xin nghỉ phép mới. Trạng thái mặc định là "Pending".</summary>
        /// <param name="dto">Thông tin đơn xin nghỉ phép.</param>
        /// <response code="200">Gửi đơn thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ, EmployeeId không tồn tại, hoặc EndDate trước StartDate.</response>
        // POST /api/leaverequests
        [HttpPost]
        public async Task<ActionResult<LeaveRequestDto>> Create(LeaveRequestCreateDto dto)
        {
            return Ok(await _leaveRequestService.CreateAsync(dto));
        }

        /// <summary>HR duyệt hoặc từ chối một đơn xin nghỉ phép. (Admin, HR)</summary>
        /// <param name="id">Id của đơn xin nghỉ phép.</param>
        /// <param name="dto">Trạng thái mới ("Approved" hoặc "Rejected").</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy đơn.</response>
        /// <response code="400">Status không phải "Approved" hoặc "Rejected".</response>
        // PUT /api/leaverequests/1/status
        [Authorize(Policy = "AdminOrHR")]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, LeaveRequestUpdateStatusDto dto)
        {
            await _leaveRequestService.UpdateStatusAsync(id, dto);
            return NoContent();
        }
    }
}
