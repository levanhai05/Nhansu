using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhanSu.Models
{
    public class AttendanceRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        // Ngày làm việc theo múi giờ Việt Nam
        public DateTime WorkDate { get; set; }

        // Thời gian lưu trong database theo UTC
        public DateTime CheckInUtc { get; set; }

        public DateTime? CheckOutUtc { get; set; }

        // Số phút đi muộn
        public int LateMinutes { get; set; }

        // Số phút về sớm
        public int EarlyLeaveMinutes { get; set; }

        // Tổng số phút làm việc
        public int WorkedMinutes { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        // Snapshot ca làm việc tại thời điểm chấm công.
        public int? WorkShiftId { get; set; }

        [MaxLength(100)]
        public string? ShiftName { get; set; }

        public TimeSpan? ShiftStartTime { get; set; }

        public TimeSpan? ShiftEndTime { get; set; }

        public TimeSpan? ShiftBreakStartTime { get; set; }

        public TimeSpan? ShiftBreakEndTime { get; set; }

        public int? ShiftGraceMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

