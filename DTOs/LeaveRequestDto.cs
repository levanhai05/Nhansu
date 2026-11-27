using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs
{
    /// <summary>Thông tin đơn xin nghỉ phép trả về cho client.</summary>
    public class LeaveRequestDto
    {
        /// <summary>Id đơn xin nghỉ phép.</summary>
        public int Id { get; set; }

        /// <summary>Id nhân viên gửi đơn.</summary>
        public int EmployeeId { get; set; }

        /// <summary>Tên nhân viên gửi đơn.</summary>
        public string? EmployeeName { get; set; }

        /// <summary>Ngày bắt đầu nghỉ.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Ngày kết thúc nghỉ.</summary>
        public DateTime EndDate { get; set; }

        /// <summary>Lý do xin nghỉ.</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>Trạng thái đơn (Pending, Approved, Rejected).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Thời điểm tạo đơn.</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Dữ liệu đầu vào để nhân viên gửi đơn xin nghỉ phép.</summary>
    public class LeaveRequestCreateDto
    {
        /// <summary>Id nhân viên. Bắt buộc, phải tồn tại trong hệ thống.</summary>
        [Required]
        public int EmployeeId { get; set; }

        /// <summary>Ngày bắt đầu nghỉ. Bắt buộc.</summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>Ngày kết thúc nghỉ. Bắt buộc, phải sau ngày bắt đầu.</summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>Lý do xin nghỉ. Bắt buộc, tối đa 500 ký tự.</summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Dữ liệu đầu vào để HR duyệt hoặc từ chối đơn xin nghỉ phép.</summary>
    public class LeaveRequestUpdateStatusDto
    {
        /// <summary>Trạng thái mới. Chỉ nhận giá trị "Approved" hoặc "Rejected".</summary>
        [Required]
        [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "Status chỉ nhận giá trị Approved hoặc Rejected.")]
        public string Status { get; set; } = string.Empty; // Approved / Rejected
    }
}
