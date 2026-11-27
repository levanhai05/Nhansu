namespace NhansuFE.Models;

public class AttendanceRecordVm
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public DateTime WorkDate { get; set; }
    public DateTime CheckInUtc { get; set; }
    public DateTime? CheckOutUtc { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public string Status { get; set; } = "";
    public string? Note { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan? ShiftStartTime { get; set; }
    public TimeSpan? ShiftEndTime { get; set; }
}

public class AttendanceSummaryVm
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
    public List<AttendanceRecordVm> Records { get; set; } = new();
}

public class WorkShiftVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public int GraceMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class WorkShiftCreateVm
{
    public string Name { get; set; } = "";
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public int GraceMinutes { get; set; } = 5;
}

public class EmployeeShiftAssignmentVm
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public int WorkShiftId { get; set; }
    public string WorkShiftName { get; set; } = "";
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrent { get; set; }
}

public class AssignWorkShiftVm
{
    public int EmployeeId { get; set; }
    public int WorkShiftId { get; set; }
    public DateTime EffectiveFrom { get; set; }
}
