using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Dữ liệu tạo tài khoản mới.</summary>
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Username không được để trống.")]
        [StringLength(20, ErrorMessage = "Username tối đa 20 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 12,
            ErrorMessage = "Mật khẩu phải từ 12 đến 100 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Role { get; set; }
    }

    /// <summary>Thông tin người dùng trả về cho client.</summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}