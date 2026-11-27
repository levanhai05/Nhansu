using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.Settings
{
    /// <summary>Cấu hình JWT, bind từ section "JwtSettings" (Options Pattern).</summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        /// <summary>Khoá ký token (HmacSha256 cần tối thiểu 32 ký tự).</summary>
        [Required, MinLength(32)]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        /// <summary>Thời gian sống của token (phút).</summary>
        [Range(1, 1440)]
        public int ExpiryMinutes { get; set; } = 60;
    }
}
