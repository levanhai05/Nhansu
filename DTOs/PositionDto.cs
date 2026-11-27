using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Thông tin Chức vụ trả về cho client.</summary>
    public class PositionDto
    {
        /// <summary>Id Chức vụ.</summary>
        public int Id { get; set; }

        /// <summary>Tên Chức vụ.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Lương cơ bản theo Chức vụ.</summary>
        public decimal BaseSalary { get; set; }
    }

    /// <summary>Dữ liệu đầu vào để tạo mới hoặc cập nhật một Chức vụ.</summary>
    public class PositionCreateDto
    {
        /// <summary>Tên Chức vụ. Bắt buộc, tối đa 100 ký tự.</summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Lương cơ bản. Bắt buộc, không được âm.</summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản không được âm.")]
        public decimal BaseSalary { get; set; }
    }
}
