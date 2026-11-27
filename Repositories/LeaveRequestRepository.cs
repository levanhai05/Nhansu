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

        public async Task<List<LeaveRequest>> GetAllAsync()
        {
            return await _db.LeaveRequests
                .AsNoTracking()
                .Include(l => l.Employee)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
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

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
