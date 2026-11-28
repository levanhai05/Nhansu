using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AppDbContext _db;

        public AttendanceRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AttendanceRecord?> GetByEmployeeAndDateAsync(
            int employeeId,
            DateTime workDate)
        {
            return await _db.Set<AttendanceRecord>()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.WorkDate == workDate);
        }

        public async Task<List<AttendanceRecord>> GetMonthAsync(
            DateTime startDate,
            DateTime endDate,
            int? employeeId = null)
        {
            var query = _db.Set<AttendanceRecord>()
                .AsNoTracking()
                .Include(x => x.Employee)
                .Where(x =>
                    x.WorkDate >= startDate &&
                    x.WorkDate < endDate);

            if (employeeId.HasValue)
            {
                query = query.Where(x =>
                    x.EmployeeId == employeeId.Value);
            }

            return await query
                .OrderBy(x => x.WorkDate)
                .ThenBy(x => x.EmployeeId)
                .ToListAsync();
        }

        public async Task AddAsync(AttendanceRecord record)
        {
            await _db.Set<AttendanceRecord>().AddAsync(record);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
