namespace QLNhanSu.DTOs.Attendance
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public DateTime CheckInUtc { get; set; }
        public DateTime? CheckOutUtc { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int WorkedMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? ShiftName { get; set; }
        public TimeSpan? ShiftStartTime { get; set; }
        public TimeSpan? ShiftEndTime { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalWorkDays { get; set; }
        public int CompletedDays { get; set; }
        public int MissingCheckoutDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyLeaveDays { get; set; }
        public int TotalLateMinutes { get; set; }
        public int TotalEarlyLeaveMinutes { get; set; }
        public int TotalWorkedMinutes { get; set; }
        public List<AttendanceDto> Records { get; set; } = new();
    }
}
