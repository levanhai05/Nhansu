using QLNhanSu.DTOs.WorkShifts;

namespace QLNhanSu.Services
{
    public interface IWorkShiftService
    {
        Task<List<WorkShiftDto>> GetShiftsAsync(bool includeInactive = false);
        Task<WorkShiftDto> CreateAsync(WorkShiftCreateDto dto);
        Task UpdateAsync(int id, WorkShiftCreateDto dto);
        Task DeactivateAsync(int id);
        Task<List<EmployeeShiftAssignmentDto>> GetAssignmentsAsync(int? employeeId = null);
        Task AssignAsync(AssignWorkShiftDto dto);
        Task<WorkShiftDto?> GetMyCurrentShiftAsync();
    }
}
