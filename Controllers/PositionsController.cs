using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs;
using QLNhanSu.Services;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý Chức vụ (Giảng viên, Trưởng khoa, Nhân viên văn phòng...).
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        /// <summary>Lấy danh sách toàn bộ Chức vụ.</summary>
        [HttpGet]
        public async Task<ActionResult<List<PositionDto>>> GetAll()
        {
            return Ok(await _positionService.GetAllAsync());
        }

        /// <summary>Lấy thông tin một Chức vụ theo Id.</summary>
        /// <param name="id">Id của Chức vụ.</param>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PositionDto>> GetById(int id)
        {
            return Ok(await _positionService.GetByIdAsync(id));
        }

        /// <summary>Tạo mới một Chức vụ. (Admin, HR)</summary>
        /// <param name="dto">Thông tin Chức vụ cần tạo.</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        /// <response code="403">Không đủ quyền.</response>
        [Authorize(Policy = "AdminOrHR")]
        [HttpPost]
        public async Task<ActionResult<PositionDto>> Create(PositionCreateDto dto)
        {
            var created = await _positionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Cập nhật thông tin một Chức vụ. (Admin, HR)</summary>
        /// <param name="id">Id của Chức vụ cần cập nhật.</param>
        /// <param name="dto">Thông tin mới.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy.</response>
        [Authorize(Policy = "AdminOrHR")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, PositionCreateDto dto)
        {
            await _positionService.UpdateAsync(id, dto);
            return NoContent();
        }

        /// <summary>Xoá một Chức vụ (chỉ khi không còn nhân viên). (Admin)</summary>
        /// <param name="id">Id của Chức vụ cần xoá.</param>
        /// <response code="204">Xoá thành công.</response>
        /// <response code="400">Chức vụ còn nhân viên.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _positionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
