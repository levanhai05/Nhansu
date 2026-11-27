// Đặt tại: Controllers/EmployeesController.cs (thay file cũ)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs;
using QLNhanSu.DTOs.Common;
using QLNhanSu.Services;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý thông tin nhân viên: xem danh sách, xem chi tiết, thêm mới, cập nhật, ngừng hoạt động.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly CurrentUserService _current;

        public EmployeesController(IEmployeeService employeeService, CurrentUserService current)
        {
            _employeeService = employeeService;
            _current = current;
        }

        /// <summary>Lấy danh sách nhân viên có phân trang, tìm kiếm, lọc và sắp xếp.</summary>
        /// <param name="queryParams">PageNumber, PageSize (tối đa 50), SearchTerm (họ tên/email), DepartmentId, PositionId, IsActive, SortBy (fullname, email, hiredate), IsDescending.</param>
        /// <response code="200">Danh sách nhân viên đã phân trang.</response>
        // GET /api/employees?pageNumber=1&pageSize=10&searchTerm=an&departmentId=1
        [Authorize(Policy = "AdminOrHR")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<EmployeeDto>>> GetAll([FromQuery] EmployeeQueryParameters queryParams)
        {
            var actor = await _current.GetAsync();
            if (actor.Role is not ("Admin" or "HR")) return Forbid();
            return Ok(await _employeeService.GetAllAsync(queryParams));
        }

        /// <summary>Lấy thông tin chi tiết một nhân viên theo Id.</summary>
        /// <param name="id">Id của nhân viên.</param>
        /// <response code="404">Không tìm thấy nhân viên.</response>
        // GET /api/employees/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id)
        {
            var actor = await _current.GetAsync();
            if (actor.Role == "User" && await _current.EmployeeIdAsync(actor.Id) != id)
                return NotFound();
            return Ok(await _employeeService.GetByIdAsync(id));
        }

        [HttpGet("me")]
        public async Task<ActionResult<EmployeeDto>> GetMe()
        {
            var actor = await _current.GetAsync();
            return Ok(await _employeeService.GetByIdAsync(await _current.EmployeeIdAsync(actor.Id)));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id:int}/link-user/{userId:int}")]
        public async Task<IActionResult> LinkUser(int id, int userId)
        {
            await _current.LinkAsync(id, userId);
            return NoContent();
        }

        /// <summary>Hủy liên kết với tài khoản hiện tại. Chỉ Admin.</summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id:int}/link-user/{userId:int}")]
        public async Task<IActionResult> UnlinkUser(int id, int userId)
        {
            await _current.UnlinkAsync(id, userId);
            return NoContent();
        }

        /// <summary>Thay liên kết bằng tài khoản chưa liên kết. Chỉ Admin.</summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id:int}/link-user/{oldUserId:int}/replace/{newUserId:int}")]
        public async Task<IActionResult> ReplaceUser(int id, int oldUserId, int newUserId)
        {
            await _current.ReplaceLinkAsync(id, oldUserId, newUserId);
            return NoContent();
        }

        /// <summary>Tạo mới một nhân viên. (Admin, HR)</summary>
        /// <param name="dto">Thông tin nhân viên cần tạo.</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc DepartmentId/PositionId không tồn tại.</response>
        /// <response code="403">Không đủ quyền.</response>
        // POST /api/employees
        [Authorize(Policy = "AdminOrHR")]
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> Create(EmployeeCreateDto dto)
        {
            var actor = await _current.GetAsync();
            if (actor.Role is not ("Admin" or "HR")) return Forbid();
            var created = await _employeeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Cập nhật thông tin một nhân viên. (Admin, HR)</summary>
        /// <param name="id">Id của nhân viên cần cập nhật.</param>
        /// <param name="dto">Thông tin mới.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="400">DepartmentId/PositionId không tồn tại.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy nhân viên.</response>
        // PUT /api/employees/1
        [Authorize(Policy = "AdminOrHR")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, EmployeeCreateDto dto)
        {
            var actor = await _current.GetAsync();
            if (actor.Role is not ("Admin" or "HR")) return Forbid();
            await _employeeService.UpdateAsync(id, dto);
            return NoContent();
        }

        /// <summary>Cho nhân viên nghỉ việc (IsActive = false), không xoá khỏi hệ thống. (Admin)</summary>
        /// <param name="id">Id của nhân viên.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy nhân viên.</response>
        // DELETE /api/employees/1
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var actor = await _current.GetAsync();
            if (actor.Role != "Admin") return Forbid();
            await _employeeService.DeactivateAsync(id);
            return NoContent();
        }
    }
}