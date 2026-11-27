// Đặt tại: Controllers/DepartmentsController.cs (thay toàn bộ nội dung file hiện tại)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs;
using QLNhanSu.Services;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý Khoa/Phòng ban: xem danh sách, xem chi tiết, thêm, sửa, xoá.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        /// <summary>Lấy danh sách Khoa/Phòng ban.</summary>
        /// <response code="200">Trả về danh sách.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        // GET /api/departments
        [HttpGet]
        public async Task<ActionResult<List<DepartmentDto>>> GetAll()
        {
            return Ok(await _departmentService.GetAllAsync());
        }

        /// <summary>Lấy chi tiết một Khoa/Phòng ban theo Id.</summary>
        /// <param name="id">Id của Khoa/Phòng ban.</param>
        /// <response code="200">Trả về thông tin Khoa/Phòng ban.</response>
        /// <response code="404">Không tìm thấy.</response>
        // GET /api/departments/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DepartmentDto>> GetById(int id)
        {
            return Ok(await _departmentService.GetByIdAsync(id));
        }

        /// <summary>Tạo mới Khoa/Phòng ban. (Admin, HR)</summary>
        /// <param name="dto">Tên và mô tả.</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc trùng tên.</response>
        /// <response code="403">Không đủ quyền.</response>
        // POST /api/departments
        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> Create(DepartmentCreateDto dto)
        {
            var created = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Cập nhật Khoa/Phòng ban. (Admin, HR)</summary>
        /// <param name="id">Id cần cập nhật.</param>
        /// <param name="dto">Thông tin mới.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc trùng tên.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy.</response>
        // PUT /api/departments/1
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, DepartmentCreateDto dto)
        {
            await _departmentService.UpdateAsync(id, dto);
            return NoContent();
        }

        /// <summary>Xoá Khoa/Phòng ban (chỉ khi không còn nhân viên). (Admin)</summary>
        /// <param name="id">Id cần xoá.</param>
        /// <response code="204">Xoá thành công.</response>
        /// <response code="400">Khoa/Phòng ban còn nhân viên.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy.</response>
        // DELETE /api/departments/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _departmentService.DeleteAsync(id);
            return NoContent();
        }
    }
}