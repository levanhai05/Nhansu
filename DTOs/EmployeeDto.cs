using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Thông tin nhân viên trả về cho client.</summary>
    public class EmployeeDto
    {
        /// <summary>Id nhân viên.</summary>
        public int Id { get; set; }

        /// <summary>Họ tên đầy đủ.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Ngày sinh.</summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>Giới tính.</summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>Email liên hệ.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Số điện thoại liên hệ.</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Địa chỉ.</summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>Ngày vào làm.</summary>
        public DateTime HireDate { get; set; }

        /// <summary>Trạng thái đang làm việc hay đã nghỉ việc.</summary>
        public bool IsActive { get; set; }

        /// <summary>Id Khoa/Phòng ban.</summary>
        public int DepartmentId { get; set; }

        /// <summary>Tên Khoa/Phòng ban.</summary>
        public string? DepartmentName { get; set; }

        /// <summary>Id Chức vụ.</summary>
        public int PositionId { get; set; }

        /// <summary>Tên Chức vụ.</summary>
        public string? PositionName { get; set; }
    }

    /// <summary>Dữ liệu đầu vào để tạo mới hoặc cập nhật một nhân viên.</summary>
    public class EmployeeCreateDto
    {
        /// <summary>Họ tên đầy đủ. Bắt buộc, tối đa 100 ký tự.</summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Ngày sinh. Bắt buộc.</summary>
        [Required]
        public DateTime DateOfBirth { get; set; }

        /// <summary>Giới tính. Bắt buộc, tối đa 10 ký tự.</summary>
        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        /// <summary>Email liên hệ. Bắt buộc, phải đúng định dạng email, tối đa 100 ký tự.</summary>
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>Số điện thoại liên hệ. Bắt buộc, phải đúng định dạng số điện thoại, tối đa 20 ký tự.</summary>
        [Required]
        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>Địa chỉ. Tối đa 200 ký tự.</summary>
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        /// <summary>Ngày vào làm. Bắt buộc.</summary>
        [Required]
        public DateTime HireDate { get; set; }

        /// <summary>Id Khoa/Phòng ban. Bắt buộc, phải tồn tại trong hệ thống.</summary>
        [Required]
        public int DepartmentId { get; set; }

        /// <summary>Id Chức vụ. Bắt buộc, phải tồn tại trong hệ thống.</summary>
        [Required]
        public int PositionId { get; set; }
    }
}
