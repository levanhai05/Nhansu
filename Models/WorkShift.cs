using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhanSu.Models
{
    public class WorkShift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public TimeSpan? BreakStartTime { get; set; }

        public TimeSpan? BreakEndTime { get; set; }

        public int GraceMinutes { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<EmployeeShiftAssignment> Assignments { get; set; }
            = new List<EmployeeShiftAssignment>();
    }
}
