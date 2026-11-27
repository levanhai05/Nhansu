using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhanSu.Models
{
    public class EmployeeShiftAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required]
        public int WorkShiftId { get; set; }

        public WorkShift? WorkShift { get; set; }

        // Ngày bắt đầu áp dụng, theo ngày Việt Nam.
        public DateTime EffectiveFrom { get; set; }

        // Ngày kết thúc áp dụng, null nghĩa là chưa kết thúc.
        public DateTime? EffectiveTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
