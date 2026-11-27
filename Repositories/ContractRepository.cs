using QLNhanSu.DTOs.Common;
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

        public async Task<PagedResult<Contract>> GetAllAsync(PaginationParameters page, int? employeeId = null)
        {
            var query = _db.Contracts.AsNoTracking().AsQueryable();
            if (employeeId.HasValue) query = query.Where(x => x.EmployeeId == employeeId.Value);
            var total = await query.CountAsync();
            var items = await query.Include(x => x.Employee)
                .OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id)
                .Skip(page.Offset).Take(page.PageSize).ToListAsync();
            return new PagedResult<Contract>(items, total, page.PageNumber, page.PageSize);
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
