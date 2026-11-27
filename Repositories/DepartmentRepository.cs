// Đặt tại: Repositories/DepartmentRepository.cs
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _db;

        public DepartmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _db.Departments
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _db.Departments.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await _db.Departments.AnyAsync(d =>
                d.Name.ToLower() == normalized && (excludeId == null || d.Id != excludeId));
        }

        public async Task<bool> HasEmployeesAsync(int id)
        {
            return await _db.Employees.AnyAsync(e => e.DepartmentId == id);
        }

        public async Task AddAsync(Department department)
        {
            await _db.Departments.AddAsync(department);
        }

        public void Remove(Department department)
        {
            _db.Departments.Remove(department);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}