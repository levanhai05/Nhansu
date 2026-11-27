using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
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

            if (memoryStream.Length > _settings.MaxFileSizeInBytes)
                throw new ArgumentException("File vượt quá dung lượng cho phép.");
            var content = memoryStream.ToArray();
            string detectedMime;
            try
            {
                var format = Image.DetectFormat(content);
                var expected = extension switch
                {
                    ".jpg" or ".jpeg" => "JPEG",
                    ".png" => "PNG",
                    ".gif" => "GIF",
                    ".webp" => "WEBP",
                    ".bmp" => "BMP",
                    _ => ""
                };
                if (!string.Equals(format.Name, expected, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Nội dung ảnh không khớp phần mở rộng.");
                var info = Image.Identify(content);
                if (info.Width <= 0 || info.Height <= 0 ||
                    (long)info.Width * info.Height > 12_000_000)
                    throw new ArgumentException("Kích thước ảnh không hợp lệ hoặc quá lớn.");
                using var image = Image.Load(new DecoderOptions { MaxFrames = 1, SkipMetadata = true }, content);
                // Mã hóa lại ảnh, loại bỏ dữ liệu đính kèm và metadata không cần thiết.
                using var clean = new MemoryStream();
                image.Save(clean, format);
                content = clean.ToArray();
                if (content.LongLength > _settings.MaxFileSizeInBytes)
                    throw new ArgumentException("Ảnh sau xử lý vượt quá dung lượng cho phép.");
                detectedMime = format.DefaultMimeType;
            }
            catch (UnknownImageFormatException)
            { throw new ArgumentException("File không phải ảnh hợp lệ."); }
            catch (InvalidImageContentException)
            { throw new ArgumentException("Dữ liệu ảnh bị hỏng."); }
            catch (NotSupportedException)
            { throw new ArgumentException("Định dạng ảnh không được hỗ trợ."); }

            var storedFile = new StoredFile
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = detectedMime,
                Content = content,
                Length = content.LongLength
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