using QLNhanSu.DTOs.WorkShifts;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class WorkShiftService : IWorkShiftService
    {
        private readonly IWorkShiftRepository _repo;
        private readonly CurrentUserService _current;

        public WorkShiftService(IWorkShiftRepository repo, CurrentUserService current)
        {
            _repo = repo;
            _current = current;
        }

        private static void Validate(WorkShiftCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new BadRequestException("Tên ca làm việc là bắt buộc.");

            if (dto.StartTime < TimeSpan.Zero || dto.StartTime >= TimeSpan.FromDays(1) ||
                dto.EndTime <= dto.StartTime || dto.EndTime > TimeSpan.FromDays(1))
                throw new BadRequestException("Giờ kết thúc phải sau giờ bắt đầu trong cùng một ngày.");

            if ((dto.BreakStartTime.HasValue && !dto.BreakEndTime.HasValue) ||
                (!dto.BreakStartTime.HasValue && dto.BreakEndTime.HasValue))
                throw new BadRequestException("Phải nhập cả giờ bắt đầu và kết thúc nghỉ.");

            if (dto.BreakStartTime.HasValue &&
                (dto.BreakStartTime < dto.StartTime ||
                 dto.BreakEndTime <= dto.BreakStartTime ||
                 dto.BreakEndTime > dto.EndTime))
                throw new BadRequestException("Khung giờ nghỉ phải nằm trong ca làm việc.");

            if (dto.GraceMinutes < 0 || dto.GraceMinutes > 60)
                throw new BadRequestException("Thời gian ân hạn phải từ 0 đến 60 phút.");
        }

        private static WorkShiftDto ToDto(WorkShift x) => new()
        {
            Id = x.Id, Name = x.Name, StartTime = x.StartTime, EndTime = x.EndTime,
            BreakStartTime = x.BreakStartTime, BreakEndTime = x.BreakEndTime,
            GraceMinutes = x.GraceMinutes, IsActive = x.IsActive
        };

        public async Task<List<WorkShiftDto>> GetShiftsAsync(bool includeInactive = false)
        {
            var actor = await _current.GetAsync();
            if (includeInactive && actor.Role is not ("Admin" or "HR"))
                throw new AppException("Bạn không có quyền xem ca đã ngừng hoạt động.", 403);

            return (await _repo.GetWorkShiftsAsync(includeInactive)).Select(ToDto).ToList();
        }

        public async Task<WorkShiftDto> CreateAsync(WorkShiftCreateDto dto)
        {
            EnsureHr(await _current.GetAsync());
            Validate(dto);

            var shift = new WorkShift
            {
                Name = dto.Name.Trim(), StartTime = dto.StartTime, EndTime = dto.EndTime,
                BreakStartTime = dto.BreakStartTime, BreakEndTime = dto.BreakEndTime,
                GraceMinutes = dto.GraceMinutes, IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddWorkShiftAsync(shift);
            await _repo.SaveChangesAsync();
            return ToDto(shift);
        }

        public async Task UpdateAsync(int id, WorkShiftCreateDto dto)
        {
            EnsureHr(await _current.GetAsync());
            Validate(dto);

            var shift = await _repo.GetWorkShiftByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy ca làm việc ID {id}.");

            shift.Name = dto.Name.Trim();
            shift.StartTime = dto.StartTime;
            shift.EndTime = dto.EndTime;
            shift.BreakStartTime = dto.BreakStartTime;
            shift.BreakEndTime = dto.BreakEndTime;
            shift.GraceMinutes = dto.GraceMinutes;
            await _repo.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            EnsureHr(await _current.GetAsync());

            var shift = await _repo.GetWorkShiftByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy ca làm việc ID {id}.");

            var today = VietnamToday();
            var assignments = await _repo.GetAssignmentsAsync();

            var isStillAssigned = assignments.Any(x =>
                x.WorkShiftId == id &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= today));

            if (isStillAssigned)
                throw new BadRequestException(
                    "Không thể ngừng ca đang được phân cho nhân viên. Hãy điều chỉnh lịch phân ca trước.");

            shift.IsActive = false;
            await _repo.SaveChangesAsync();
        }

        public async Task<List<EmployeeShiftAssignmentDto>> GetAssignmentsAsync(int? employeeId = null)
        {
            EnsureHr(await _current.GetAsync());
            var today = VietnamToday();

            return (await _repo.GetAssignmentsAsync(employeeId)).Select(x => new EmployeeShiftAssignmentDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee?.FullName ?? "",
                WorkShiftId = x.WorkShiftId,
                WorkShiftName = x.WorkShift?.Name ?? "",
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                IsCurrent = x.EffectiveFrom.Date <= today &&
                    (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= today)
            }).ToList();
        }

        public async Task AssignAsync(AssignWorkShiftDto dto)
        {
            EnsureHr(await _current.GetAsync());

            var effectiveFrom = dto.EffectiveFrom.Date;
            if (effectiveFrom == default)
                throw new BadRequestException("Ngày bắt đầu hiệu lực không hợp lệ.");

            var employee = await _repo.GetEmployeeByIdAsync(dto.EmployeeId)
                ?? throw new NotFoundException("Không tìm thấy nhân viên.");

            if (!employee.IsActive)
                throw new BadRequestException("Không thể phân ca cho nhân viên đã ngừng hoạt động.");

            var shift = await _repo.GetWorkShiftByIdAsync(dto.WorkShiftId)
                ?? throw new NotFoundException("Không tìm thấy ca làm việc.");

            if (!shift.IsActive)
                throw new BadRequestException("Không thể phân ca đã ngừng hoạt động.");

            var assignments = await _repo.GetAssignmentsAsync(dto.EmployeeId);

            // Không cho phép chèn lịch mới vào trước một lịch phân ca đã được lên kế hoạch.
            if (assignments.Any(x => x.EffectiveFrom.Date >= effectiveFrom))
                throw new BadRequestException("Đã có lịch phân ca bắt đầu từ ngày này hoặc sau đó. Hãy kiểm tra lịch sử trước khi phân ca.");

            var openOrOverlapping = assignments
                .Where(x => !x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= effectiveFrom)
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefault();

            if (openOrOverlapping != null)
            {
                if (openOrOverlapping.EffectiveFrom.Date >= effectiveFrom)
                    throw new BadRequestException("Lịch phân ca bị trùng ngày hiệu lực.");

                openOrOverlapping.EffectiveTo = effectiveFrom.AddDays(-1);
            }

            await _repo.AddAssignmentAsync(new EmployeeShiftAssignment
            {
                EmployeeId = dto.EmployeeId,
                WorkShiftId = dto.WorkShiftId,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = null,
                CreatedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();
        }

        public async Task<WorkShiftDto?> GetMyCurrentShiftAsync()
        {
            var actor = await _current.GetAsync();
            var employeeId = await _current.EmployeeIdAsync(actor.Id);
            var assignment = await _repo.GetAssignmentForDateAsync(employeeId, VietnamToday());
            return assignment?.WorkShift == null ? null : ToDto(assignment.WorkShift);
        }

        private static DateTime VietnamToday()
        {
            TimeZoneInfo zone;
            try { zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
            catch { zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone).Date;
        }

        private static void EnsureHr((int Id, string Role) actor)
        {
            if (actor.Role is not ("Admin" or "HR"))
                throw new AppException("Bạn không có quyền thực hiện thao tác này.", 403);
        }
    }
}
