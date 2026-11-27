namespace QLNhanSu.DTOs
{
    public class FileUploadDbDto
    {
        public class UploadFileDto
        {
            /// <summary>Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp, bmp). Dung lượng tối đa 1MB.</summary>
            public required IFormFile File { get; set; }
        }

        public class FileResponseDto
        {
            public int Id { get; set; }
            public string FileName { get; set; } = string.Empty;
            public long Size { get; set; }
            public DateTime UploadedAt { get; set; }
        }

        public class FileDownloadDto
        {
            public required byte[] Content { get; set; }
            public required string ContentType { get; set; }
            public required string FileName { get; set; }
        }
    }
}