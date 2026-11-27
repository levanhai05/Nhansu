using Microsoft.AspNetCore.Mvc;
using QLNhanSu.Services;
using static QLNhanSu.DTOs.FileUploadDbDto;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Upload và tải file ảnh (lưu trực tiếp trong database).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>Upload một file ảnh (jpg, jpeg, png, gif, webp, bmp), dung lượng tối đa 1MB.</summary>
        /// <param name="dto">File ảnh cần upload (multipart/form-data).</param>
        /// <response code="200">Upload thành công, trả về Id, tên file, dung lượng và thời điểm upload.</response>
        /// <response code="400">File rỗng, vượt quá 1MB hoặc không phải file ảnh.</response>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadFileDto dto)
        {
            // ArgumentException / InvalidOperationException từ service
            // sẽ được ExceptionHandlingMiddleware map thành 400.
            var result = await _fileService.UploadFileAsync(dto.File);
            return Ok(result);
        }

        /// <summary>Tải về một file đã upload theo Id.</summary>
        /// <param name="id">Id của file.</param>
        /// <response code="200">Trả về nội dung file.</response>
        /// <response code="404">Không tìm thấy file với Id tương ứng.</response>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            var fileDto = await _fileService.GetFileForDownloadAsync(id);

            if (fileDto == null)
                throw new Exceptions.NotFoundException("Không tìm thấy file yêu cầu.");

            return File(fileDto.Content, fileDto.ContentType, fileDto.FileName);
        }
    }
}