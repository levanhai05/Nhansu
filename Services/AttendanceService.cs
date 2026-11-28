using Microsoft.EntityFrameworkCore;
using QLNhanSu.DTOs.Attendance;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IWorkShiftRepository _shiftRepo;
        private readonly CurrentUserService _current;

        private static readonly TimeZoneInfo VietnamTimeZone = GetVietnamTimeZone();

        public AttendanceService(
            IAttendanceRepository repo,
            IWorkShiftRepository shiftRepo,
            CurrentUserService current)
        {
            _repo = repo;
            _shiftRepo = shiftRepo;
            _current = current;
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        }

        private static DateTime VietnamNow() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

        private static DateTime ToVietnam(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), VietnamTimeZone);

        private static void ValidateMonth(int year, int month)
        {
            if (year < 2000 || year > 2100 || month < 1 || month > 12)
                throw new BadRequestException("Tháng hoặc năm không hợp lệ.");
        }

        public async Task<AttendanceDto> CheckInAsync()
        {
            var actor = await _current.GetAsync();
            var employeeId = await _current.EmployeeIdAsync(actor.Id);
            var now = VietnamNow();
            var date = now.Date;

            var existing = await _repo.GetByEmployeeAndDateAsync(employeeId, date);
            if (existing != null)
                throw new AppException("Bạn đã chấm công vào hôm nay.", 409);

            var assignment = await _shiftRepo.GetAssignmentForDateAsync(employeeId, date);
            var shift = assignment?.WorkShift;
            if (shift == null || !shift.IsActive)
                throw new BadRequestException("Bạn chưa được phân ca làm việc có hiệu lực hôm nay. Vui lòng liên hệ Admin/HR.");

            var scheduledStart = date.Add(shift.StartTime);
            if (now < scheduledStart)
                throw new BadRequestException($"Chưa đến giờ chấm công. Ca của bạn bắt đầu lúc {shift.StartTime:hh\\:mm}.");

            var graceEnd = scheduledStart.AddMinutes(shift.GraceMinutes);
            var late = now > graceEnd
                ? (int)Math.Ceiling((now - graceEnd).TotalMinutes)
                : 0;

            var utcNow = DateTime.UtcNow;
            var record = new AttendanceRecord
            {
                EmployeeId = employeeId,
                WorkDate = date,
                CheckInUtc = utcNow,
                LateMinutes = late,
                EarlyLeaveMinutes = 0,
                WorkedMinutes = 0,
                WorkShiftId = shift.Id,
                ShiftName = shift.Name,
                ShiftStartTime = shift.StartTime,
                ShiftEndTime = shift.EndTime,
                ShiftBreakStartTime = shift.BreakStartTime,
                ShiftBreakEndTime = shift.BreakEndTime,
                ShiftGraceMinutes = shift.GraceMinutes,
                CreatedAt = utcNow
            };

            await _repo.AddAsync(record);
            try { await _repo.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                throw new AppException("Bạn đã chấm công vào hôm nay.", 409);
            }

            return ToDto(record, null);
        }

        public async Task<AttendanceDto> CheckOutAsync()
        {
            var actor = await _current.GetAsync();
            var employeeId = await _current.EmployeeIdAsync(actor.Id);
            var nowLocal = VietnamNow();
            var record = await _repo.GetByEmployeeAndDateAsync(employeeId, nowLocal.Date);

            if (record == null)
                throw new BadRequestException("Bạn chưa chấm công vào hôm nay.");
            if (record.CheckOutUtc.HasValue)
                throw new AppException("Bạn đã chấm công ra hôm nay.", 409);

            var nowUtc = DateTime.UtcNow;
            var checkInLocal = ToVietnam(record.CheckInUtc);
            var checkOutLocal = ToVietnam(nowUtc);
            var shiftEnd = record.ShiftEndTime.HasValue
                ? record.WorkDate.Date.Add(record.ShiftEndTime.Value)
                : record.WorkDate.Date.AddHours(17);

            record.CheckOutUtc = nowUtc;
            record.EarlyLeaveMinutes = checkOutLocal < shiftEnd
                ? (int)Math.Ceiling((shiftEnd - checkOutLocal).TotalMinutes)
                : 0;

            var total = Math.Max(0, (int)Math.Floor((checkOutLocal - checkInLocal).TotalMinutes));
            var breakMinutes = 0;
            if (record.ShiftBreakStartTime.HasValue && record.ShiftBreakEndTime.HasValue)
            {
                var breakStart = record.WorkDate.Date.Add(record.ShiftBreakStartTime.Value);
                var breakEnd = record.WorkDate.Date.Add(record.ShiftBreakEndTime.Value);
                var overlapStart = checkInLocal > breakStart ? checkInLocal : breakStart;
                var overlapEnd = checkOutLocal < breakEnd ? checkOutLocal : breakEnd;
                if (overlapEnd > overlapStart)
                    breakMinutes = (int)Math.Floor((overlapEnd - overlapStart).TotalMinutes);
            }

            record.WorkedMinutes = Math.Max(0, total - breakMinutes);
            await _repo.SaveChangesAsync();
            return ToDto(record, null);
        }

        public async Task<AttendanceSummaryDto> GetMyMonthAsync(int year, int month)
        {
            ValidateMonth(year, month);
            var actor = await _current.GetAsync();
            var employeeId = await _current.EmployeeIdAsync(actor.Id);
            var start = new DateTime(year, month, 1);
            var records = await _repo.GetMonthAsync(start, start.AddMonths(1), employeeId);
            return BuildSummary(year, month, records);
        }

        public async Task<AttendanceSummaryDto> GetAllMonthAsync(int year, int month)
        {
            ValidateMonth(year, month);
            var actor = await _current.GetAsync();
            if (actor.Role is not ("Admin" or "HR"))
                throw new AppException("Bạn không có quyền xem bảng công toàn bộ nhân viên.", 403);

            var start = new DateTime(year, month, 1);
            var records = await _repo.GetMonthAsync(start, start.AddMonths(1));
            return BuildSummary(year, month, records);
        }

        private static AttendanceSummaryDto BuildSummary(int year, int month, List<AttendanceRecord> records)
        {
            return new AttendanceSummaryDto
            {
                Year = year, Month = month,
                TotalWorkDays = records.Count,
                CompletedDays = records.Count(x => x.CheckOutUtc.HasValue),
                MissingCheckoutDays = records.Count(x => !x.CheckOutUtc.HasValue),
                LateDays = records.Count(x => x.LateMinutes > 0),
                EarlyLeaveDays = records.Count(x => x.EarlyLeaveMinutes > 0),
                TotalLateMinutes = records.Sum(x => x.LateMinutes),
                TotalEarlyLeaveMinutes = records.Sum(x => x.EarlyLeaveMinutes),
                TotalWorkedMinutes = records.Sum(x => x.WorkedMinutes),
                Records = records.Select(x => ToDto(x, x.Employee?.FullName)).ToList()
            };
        }

        private static AttendanceDto ToDto(AttendanceRecord x, string? employeeName) => new()
        {
            Id = x.Id, EmployeeId = x.EmployeeId, EmployeeName = employeeName ?? "",
            WorkDate = x.WorkDate, CheckInUtc = x.CheckInUtc, CheckOutUtc = x.CheckOutUtc,
            LateMinutes = x.LateMinutes, EarlyLeaveMinutes = x.EarlyLeaveMinutes,
            WorkedMinutes = x.WorkedMinutes,
            Status = x.CheckOutUtc.HasValue ? "Completed" : "MissingCheckout",
            Note = x.Note, ShiftName = x.ShiftName,
            ShiftStartTime = x.ShiftStartTime, ShiftEndTime = x.ShiftEndTime
        };
    }
}
