// Đặt tại: Repositories/EmployeeRepository.cs
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.DTOs.Common;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;

        public EmployeeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<Employee>> GetPagedAsync(EmployeeQueryParameters q)
        {
            var query = _db.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            // 1. SEARCH
            if (!string.IsNullOrWhiteSpace(q.SearchTerm))
            {
                var term = q.SearchTerm.Trim().ToLower();
                query = query.Where(e => e.FullName.ToLower().Contains(term)
                                      || e.Email.ToLower().Contains(term));
            }

            // 2. FILTER
            if (q.DepartmentId.HasValue)
                query = query.Where(e => e.DepartmentId == q.DepartmentId.Value);

            if (q.PositionId.HasValue)
                query = query.Where(e => e.PositionId == q.PositionId.Value);

            if (q.IsActive.HasValue)
                query = query.Where(e => e.IsActive == q.IsActive.Value);

            // 3. SORT (thêm Id để thứ tự ổn định khi phân trang)
            query = q.SortBy?.ToLower() switch
            {
                "email" => q.IsDescending
                    ? query.OrderByDescending(e => e.Email).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.Email).ThenBy(e => e.Id),

                "hiredate" => q.IsDescending
                    ? query.OrderByDescending(e => e.HireDate).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.HireDate).ThenBy(e => e.Id),

                _ => q.IsDescending
                    ? query.OrderByDescending(e => e.FullName).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.FullName).ThenBy(e => e.Id)
            };

            // 4. COUNT + 5. PAGINATION
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToListAsync();

            return new PagedResult<Employee>(items, totalCount, q.PageNumber, q.PageSize);
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Employee employee)
        {
            await _db.Employees.AddAsync(employee);
        }

        public Task<bool> DepartmentExistsAsync(int departmentId)
            => _db.Departments.AnyAsync(d => d.Id == departmentId);

        public Task<bool> PositionExistsAsync(int positionId)
            => _db.Positions.AnyAsync(p => p.Id == positionId);

        public Task<int?> GetActiveEmployeeIdByUserIdAsync(int userId)
            => _db.Employees.AsNoTracking()
                .Where(e => e.UserId == userId && e.IsActive)
                .Select(e => (int?)e.Id).SingleOrDefaultAsync();

        public Task<bool> IsUserLinkedAsync(int userId)
            => _db.Employees.AnyAsync(e => e.UserId == userId);

        public async Task<bool> ChangeUserLinkAsync(int employeeId, int? expectedUserId, int? newUserId)
        {
            var query = _db.Employees.Where(e => e.Id == employeeId && e.UserId == expectedUserId);
            if (newUserId.HasValue)
                query = query.Where(e => e.IsActive);
            return await query.ExecuteUpdateAsync(setters => setters.SetProperty(e => e.UserId, newUserId)) == 1;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}