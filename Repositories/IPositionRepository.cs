using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IPositionRepository
    {
        Task<List<Position>> GetAllAsync();
        Task<Position?> GetByIdAsync(int id);
        Task<bool> HasEmployeesAsync(int id);
        Task AddAsync(Position position);
        void Remove(Position position);
        Task<bool> SaveChangesAsync();
    }
}
