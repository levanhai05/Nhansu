using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Dữ liệu đầu vào để tạo tài khoản người dùng mới (Admin).</summary>
    public class UserRegisterDto
    {
        /// <summary>Tên đăng nhập. Bắt buộc, tối đa 20 ký tự.</summary>
        [Required(ErrorMessage = "Username không được để trống.")]
        [StringLength(20, ErrorMessage = "Username tối đa 20 ký tự.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>Mật khẩu. Bắt buộc, tối thiểu 6 ký tự.</summary>
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Email. Bắt buộc, đúng định dạng email.</summary>
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Quyền (User, HR, Admin). Bỏ trống thì mặc định là "User".</summary>
        [StringLength(20)]
        public string? Role { get; set; }
    }

    /// <summary>Dữ liệu cập nhật người dùng. Trường nào bỏ trống thì giữ nguyên giá trị cũ.</summary>
    public class UserUpdateDto
    {
        /// <summary>Tên đăng nhập mới (tối đa 20 ký tự).</summary>
        [StringLength(20, ErrorMessage = "Username tối đa 20 ký tự.")]
        public string? Username { get; set; }

        /// <summary>Mật khẩu mới (tối thiểu 6 ký tự).</summary>
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
        public string? Password { get; set; }

        /// <summary>Email mới.</summary>
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string? Email { get; set; }

        /// <summary>Quyền mới (User, HR, Admin).</summary>
        [StringLength(20)]
        public string? Role { get; set; }
    }

    /// <summary>Thông tin người dùng trả về cho client (không có mật khẩu).</summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}