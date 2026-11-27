using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Thông tin Khoa/Phòng ban trả về cho client.</summary>
    public class DepartmentDto
    {
        /// <summary>Id Khoa/Phòng ban.</summary>
        public int Id { get; set; }

        /// <summary>Tên Khoa/Phòng ban.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Mô tả Khoa/Phòng ban.</summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>Dữ liệu đầu vào để tạo mới hoặc cập nhật một Khoa/Phòng ban.</summary>
    public class DepartmentCreateDto
    {
        /// <summary>Tên Khoa/Phòng ban. Bắt buộc, tối đa 150 ký tự.</summary>
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Mô tả Khoa/Phòng ban. Tối đa 500 ký tự.</summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
