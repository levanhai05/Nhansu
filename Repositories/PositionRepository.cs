using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public class PositionRepository : IPositionRepository
    {
        private readonly AppDbContext _db;

        public PositionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Position>> GetAllAsync()
        {
            return await _db.Positions
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Position?> GetByIdAsync(int id)
        {
            return await _db.Positions.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> HasEmployeesAsync(int id)
        {
            return await _db.Employees.AnyAsync(e => e.PositionId == id);
        }

        public async Task AddAsync(Position position)
        {
            await _db.Positions.AddAsync(position);
        }

        public void Remove(Position position)
        {
            _db.Positions.Remove(position);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
