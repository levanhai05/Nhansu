using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Dữ liệu đầu vào để đăng nhập.</summary>
    public class LoginDto
    {
        /// <summary>Tên đăng nhập. Bắt buộc.</summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>Mật khẩu. Bắt buộc.</summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Dữ liệu đầu vào để đăng ký tài khoản mới.</summary>
    public class RegisterDto
    {
        /// <summary>Tên đăng nhập. Bắt buộc.</summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>Mật khẩu. Bắt buộc, tối thiểu 6 ký tự.</summary>
        [Required, MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Email. Bắt buộc, phải đúng định dạng email.</summary>
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
