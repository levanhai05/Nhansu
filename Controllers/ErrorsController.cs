using Microsoft.AspNetCore.Mvc;
using QLNhanSu.Exceptions;
using QLNhanSu.Attributes;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Endpoint dùng để kiểm tra Exception Middleware. Có thể xóa khi không cần demo.
    /// </summary>
    [DevelopmentOnly]
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorsController : ControllerBase
    {
        /// <summary>Giả lập lỗi 400 (BadRequestException) để kiểm tra Exception Middleware.</summary>
        /// <response code="400">Luôn trả về lỗi 400 dạng ErrorResponse.</response>
        [HttpGet("bad-request")]
        public IActionResult BadRequestError()
        {
            throw new BadRequestException("Đây là lỗi 400 từ Exception Middleware.");
        }

        /// <summary>Giả lập lỗi 404 (NotFoundException) để kiểm tra Exception Middleware.</summary>
        /// <response code="404">Luôn trả về lỗi 404 dạng ErrorResponse.</response>
        [HttpGet("not-found")]
        public IActionResult NotFoundError()
        {
            throw new NotFoundException("Đây là lỗi 404 từ Exception Middleware.");
        }

        /// <summary>Giả lập lỗi 401 (UnauthorizedException) để kiểm tra Exception Middleware.</summary>
        /// <response code="401">Luôn trả về lỗi 401 dạng ErrorResponse.</response>
        [HttpGet("unauthorized")]
        public IActionResult UnauthorizedError()
        {
            throw new UnauthorizedException("Đây là lỗi 401 từ Exception Middleware.");
        }

        /// <summary>Giả lập lỗi 500 (exception không lường trước) để kiểm tra Exception Middleware.</summary>
        /// <response code="500">Luôn trả về lỗi 500 dạng ErrorResponse (ẩn nội dung lỗi thật).</response>
        [HttpGet("server")]
        public IActionResult ServerError()
        {
            throw new Exception("Đây là lỗi 500 giả lập (unhandled exception).");
        }
    }
}