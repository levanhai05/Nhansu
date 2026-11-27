using QLNhanSu.Models;
using QLNhanSu.DTOs.Common;
namespace QLNhanSu.Repositories
{
    public interface IUserRepository
    {
        Task<PagedResult<User>> GetAllAsync(UserQueryParameters queryParams);
        Task<User?> GetByRefreshTokenHashAsync(string hash);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameOrEmailAsync(string username, string email);
        Task AddAsync(User user);
        void Update(User user);
        void Delete(User user);
        Task<bool> SaveChangesAsync();
    }
}