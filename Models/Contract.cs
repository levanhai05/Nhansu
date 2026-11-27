using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhanSu.Models
{
    // Entity - Hợp đồng lao động của nhân viên
    public class Contract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Required]
        [MaxLength(50)]
        public string ContractType { get; set; } = string.Empty; // Thử việc, Xác định thời hạn, Không xác định thời hạn

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lương không được âm.")]
        public decimal Salary { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";           // Active, Expired, Terminated
    }
}
