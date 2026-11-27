using Microsoft.AspNetCore.Http;
using QLNhanSu.Models;
using static QLNhanSu.DTOs.FileUploadDbDto;

namespace QLNhanSu.Services
{
    public interface IFileService
    {
        Task<FileResponseDto> UploadFileAsync(IFormFile file);
        Task<FileDownloadDto?> GetFileForDownloadAsync(int id);
    }
}