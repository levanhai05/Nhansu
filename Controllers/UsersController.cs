using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs.Common;
using QLNhanSu.Services;
using QLNhanSu.DTOs;
namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý tài khoản người dùng trong hệ thống: xem danh sách, xem chi tiết, thêm, sửa, xoá.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        // Inject IUserService thay vì AppDbContext
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Lấy danh sách người dùng có phân trang, tìm kiếm, lọc theo role và sắp xếp. (Admin, HR)</summary>
        /// <param name="queryParams">PageNumber, PageSize (tối đa 50), SearchTerm (Username/Email), Role, SortBy (username, email, createdat), IsDescending.</param>
        /// <response code="200">Trả về danh sách người dùng đã phân trang.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không đủ quyền.</response>
       [Authorize(Policy = "AdminOrHR")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAll([FromQuery] UserQueryParameters queryParams)
        {
            var result = await _userService.GetAllAsync(queryParams);
            return Ok(result);
        }

        /// <summary>Lấy thông tin một người dùng theo Id. (Admin, HR)</summary>
        /// <param name="id">Id của người dùng.</param>
        /// <response code="200">Trả về thông tin người dùng.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy người dùng.</response>
        [Authorize(Policy = "AdminOrHR")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            return Ok(user);
        }

        /// <summary>Tạo mới một tài khoản người dùng. (Admin)</summary>
        /// <param name="request">Username, Email, Password và Role (mặc định là "User" nếu bỏ trống).</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Username/Email đã được sử dụng hoặc Role không hợp lệ.</response>
        /// <response code="403">Không đủ quyền.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserRegisterDto request)
        {
            var result = await _userService.RegisterAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        /// <summary>Cập nhật thông tin một người dùng. Trường nào để trống thì giữ nguyên giá trị cũ. (Admin)</summary>
        /// <param name="id">Id của người dùng cần cập nhật.</param>
        /// <param name="request">Thông tin mới (Username, Email, Password, Role).</param>
        /// <response code="200">Cập nhật thành công.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy người dùng hoặc Role không hợp lệ.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserRegisterDto request)
        {
            var result = await _userService.UpdateAsync(id, request);
            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        /// <summary>Xoá một người dùng. (Admin)</summary>
        /// <param name="id">Id của người dùng cần xoá.</param>
        /// <response code="200">Xoá thành công.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy người dùng.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}