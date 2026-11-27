using QLNhanSu.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly AppDbContext _db;

        public LeaveRequestRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<LeaveRequest>> GetAllAsync(PaginationParameters page, int? employeeId = null)
        {
            var query = _db.LeaveRequests.AsNoTracking().AsQueryable();
            if (employeeId.HasValue) query = query.Where(x => x.EmployeeId == employeeId.Value);
            var total = await query.CountAsync();
            var items = await query.Include(x => x.Employee)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip(page.Offset).Take(page.PageSize).ToListAsync();
            return new PagedResult<LeaveRequest>(items, total, page.PageNumber, page.PageSize);
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            return await _db.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<bool> EmployeeExistsAsync(int employeeId)
        {
            return await _db.Employees.AnyAsync(e => e.Id == employeeId);
        }

        public async Task AddAsync(LeaveRequest leaveRequest)
        {
            await _db.LeaveRequests.AddAsync(leaveRequest);
        }

        public async Task<bool> TryResolvePendingAsync(int id, string status)
        {
            var affected = await _db.LeaveRequests
                .Where(l => l.Id == id && l.Status == "Pending")
                .ExecuteUpdateAsync(setters => setters.SetProperty(l => l.Status, status));
            return affected == 1;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
