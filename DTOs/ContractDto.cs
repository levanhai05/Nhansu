using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Thông tin hợp đồng lao động trả về cho client.</summary>
    public class ContractDto
    {
        /// <summary>Id hợp đồng.</summary>
        public int Id { get; set; }

        /// <summary>Id nhân viên sở hữu hợp đồng.</summary>
        public int EmployeeId { get; set; }

        /// <summary>Tên nhân viên sở hữu hợp đồng.</summary>
        public string? EmployeeName { get; set; }

        /// <summary>Loại hợp đồng (Thử việc, Xác định thời hạn, Không xác định thời hạn).</summary>
        public string ContractType { get; set; } = string.Empty;

        /// <summary>Ngày bắt đầu hợp đồng.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Ngày kết thúc hợp đồng (nếu có).</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Mức lương theo hợp đồng.</summary>
        public decimal Salary { get; set; }

        /// <summary>Trạng thái hợp đồng (Active, Expired, Terminated).</summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>Dữ liệu đầu vào để tạo mới một hợp đồng lao động.</summary>
    public class ContractCreateDto
    {
        /// <summary>Id nhân viên. Bắt buộc, phải tồn tại trong hệ thống.</summary>
        [Required]
        public int EmployeeId { get; set; }

        /// <summary>Loại hợp đồng. Bắt buộc, tối đa 50 ký tự.</summary>
        [Required]
        [MaxLength(50)]
        public string ContractType { get; set; } = string.Empty;

        /// <summary>Ngày bắt đầu hợp đồng. Bắt buộc.</summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>Ngày kết thúc hợp đồng (nếu có).</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Mức lương. Bắt buộc, không được âm.</summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lương không được âm.")]
        public decimal Salary { get; set; }
    }
}
