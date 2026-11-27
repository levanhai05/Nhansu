using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs.WorkShifts
{
    public class WorkShiftDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public int GraceMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    public class WorkShiftCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        [Range(0, 60)]
        public int GraceMinutes { get; set; } = 5;
    }

    public class AssignWorkShiftDto
    {
        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }
        [Range(1, int.MaxValue)]
        public int WorkShiftId { get; set; }
        public DateTime EffectiveFrom { get; set; }
    }

    public class EmployeeShiftAssignmentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int WorkShiftId { get; set; }
        public string WorkShiftName { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsCurrent { get; set; }
    }
}
