using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs;
using QLNhanSu.Services;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Đăng ký tài khoản và đăng nhập vào hệ thống.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Đăng ký tài khoản người dùng mới.</summary>
        /// <param name="dto">Thông tin đăng ký (username, password, email).</param>
        /// <response code="200">Đăng ký thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(new UserRegisterDto
            {
                Username = dto.Username,
                Password = dto.Password,
                Email = dto.Email
            });

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { result.Data!.Id, result.Data.Username, result.Data.Email, result.Data.Role });
        }

        /// <summary>Đăng nhập, trả về JWT token.</summary>
        /// <param name="dto">Tên đăng nhập (hoặc email) và mật khẩu.</param>
        /// <response code="200">Đăng nhập thành công, trả về token.</response>
        /// <response code="401">Sai tên đăng nhập hoặc mật khẩu.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);

            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            return Ok(new { token = result.Token });
        }
    }
}