using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IWorkShiftRepository
    {
        Task<List<WorkShift>> GetWorkShiftsAsync(bool includeInactive = false);
        Task<WorkShift?> GetWorkShiftByIdAsync(int id);
        Task AddWorkShiftAsync(WorkShift shift);

        Task<Employee?> GetEmployeeByIdAsync(int employeeId);

        Task<List<EmployeeShiftAssignment>> GetAssignmentsAsync(
            int? employeeId = null);

        Task<EmployeeShiftAssignment?> GetAssignmentForDateAsync(
            int employeeId,
            DateTime workDate);

        Task AddAssignmentAsync(EmployeeShiftAssignment assignment);

        Task<bool> SaveChangesAsync();
    }
}
