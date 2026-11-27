using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _db;

        public ContractRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Contract>> GetAllAsync()
        {
            return await _db.Contracts
                .AsNoTracking()
                .Include(c => c.Employee)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<List<Contract>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _db.Contracts
                .AsNoTracking()
                .Include(c => c.Employee)
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _db.Contracts
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> EmployeeExistsAsync(int employeeId)
        {
            return await _db.Employees.AnyAsync(e => e.Id == employeeId);
        }

        public async Task AddAsync(Contract contract)
        {
            await _db.Contracts.AddAsync(contract);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
