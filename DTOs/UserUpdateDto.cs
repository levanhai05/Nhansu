using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs;

/// <summary>Dữ liệu cập nhật tài khoản. Null nghĩa là giữ nguyên.</summary>
public class UserUpdateDto
{
    [StringLength(20, MinimumLength = 1,
        ErrorMessage = "Username phải từ 1 đến 20 ký tự.")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(100, MinimumLength = 1)]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 12,
        ErrorMessage = "Mật khẩu phải từ 12 đến 100 ký tự.")]
    public string? Password { get; set; }

    [StringLength(20, MinimumLength = 1)]
    public string? Role { get; set; }
}