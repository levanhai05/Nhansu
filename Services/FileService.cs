using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using QLNhanSu.Models;
using QLNhanSu.Repositories;
using QLNhanSu.Settings;
using static QLNhanSu.DTOs.FileUploadDbDto;

namespace QLNhanSu.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly FileSettings _settings;

        public FileService(IFileRepository fileRepository, IOptions<FileSettings> options)
        {
            _fileRepository = fileRepository;
            _settings = options.Value;
        }

        public async Task<FileResponseDto> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (file.Length > _settings.MaxFileSizeInBytes)
                throw new ArgumentException($"File vượt quá dung lượng tối đa {_settings.MaxFileSizeInMB}MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = _settings.AllowedExtensions;
            if (!allowed.Contains(extension, StringComparer.OrdinalIgnoreCase)
                || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Chỉ cho phép upload file ảnh ({string.Join(", ", allowed.Select(e => e.TrimStart('.')))}).");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var storedFile = new StoredFile
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                Content = memoryStream.ToArray(),
                Length = file.Length
            };

            var createdFile = await _fileRepository.AddAsync(storedFile);

            return new FileResponseDto
            {
                Id = createdFile.Id,
                FileName = createdFile.FileName,
                Size = createdFile.Length,
                UploadedAt = createdFile.UploadedAt
            };
        }

        public async Task<FileDownloadDto?> GetFileForDownloadAsync(int id)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null) return null;

            return new FileDownloadDto
            {
                Content = file.Content,
                ContentType = file.ContentType,
                FileName = file.FileName
            };
        }
    }
}