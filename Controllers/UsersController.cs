using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs.Common;
using QLNhanSu.Services;

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
        [Authorize(Roles = "Admin,HR")]
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
        [Authorize(Roles = "Admin,HR")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

    /// <summary>Dữ liệu đầu vào để tạo hoặc cập nhật người dùng.</summary>
    public class UserRegisterDto
    {
        /// <summary>Tên đăng nhập.</summary>
        public string Username { get; set; } = "";

        /// <summary>Email.</summary>
        public string Email { get; set; } = "";

        /// <summary>Mật khẩu.</summary>
        public string Password { get; set; } = "";

        /// <summary>Quyền của người dùng (User, HR, Admin). Bỏ trống thì mặc định là "User".</summary>
        public string? Role { get; set; }
    }

    /// <summary>Thông tin người dùng trả về cho client.</summary>
    public class UserResponseDto
    {
        /// <summary>Id người dùng.</summary>
        public int Id { get; set; }

        /// <summary>Tên đăng nhập.</summary>
        public string Username { get; set; } = "";

        /// <summary>Email.</summary>
        public string Email { get; set; } = "";

        /// <summary>Quyền của người dùng (User, HR, Admin).</summary>
        public string Role { get; set; } = "";

        /// <summary>Thời điểm tạo tài khoản.</summary>
        public DateTime CreatedAt { get; set; }
    }
}