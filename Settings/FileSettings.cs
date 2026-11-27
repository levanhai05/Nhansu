using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.Settings
{
    /// <summary>Cấu hình upload file, bind từ section "FileSettings" (Options Pattern).</summary>
    public class FileSettings
    {
        public const string SectionName = "FileSettings";

        /// <summary>Dung lượng tối đa mỗi file (MB).</summary>
        [Range(1, 100)]
        public int MaxFileSizeInMB { get; set; } = 1;

        /// <summary>Các đuôi file được phép upload (gồm dấu chấm, ví dụ ".png").</summary>
        [Required, MinLength(1)]
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

        public long MaxFileSizeInBytes => MaxFileSizeInMB * 1024L * 1024L;
    }
}
