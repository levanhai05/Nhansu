using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public class WorkShiftRepository : IWorkShiftRepository
    {
        private readonly AppDbContext _db;

        public WorkShiftRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<WorkShift>> GetWorkShiftsAsync(
            bool includeInactive = false)
        {
            var query = _db.WorkShifts.AsNoTracking();

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.StartTime)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        public Task<WorkShift?> GetWorkShiftByIdAsync(int id)
        {
            return _db.WorkShifts
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddWorkShiftAsync(WorkShift shift)
        {
            await _db.WorkShifts.AddAsync(shift);
        }

        public Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            return _db.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == employeeId);
        }

        public async Task<List<EmployeeShiftAssignment>> GetAssignmentsAsync(
            int? employeeId = null)
        {
            var query = _db.EmployeeShiftAssignments
                .Include(x => x.Employee)
                .Include(x => x.WorkShift)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(x => x.EmployeeId == employeeId.Value);

            return await query
                .OrderBy(x => x.EmployeeId)
                .ThenBy(x => x.EffectiveFrom)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        public Task<EmployeeShiftAssignment?> GetAssignmentForDateAsync(
            int employeeId,
            DateTime workDate)
        {
            return _db.EmployeeShiftAssignments
                .AsNoTracking()
                .Include(x => x.WorkShift)
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    x.EffectiveFrom <= workDate &&
                    (!x.EffectiveTo.HasValue ||
                     x.EffectiveTo.Value >= workDate))
                .OrderByDescending(x => x.EffectiveFrom)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task AddAssignmentAsync(
            EmployeeShiftAssignment assignment)
        {
            await _db.EmployeeShiftAssignments.AddAsync(assignment);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
