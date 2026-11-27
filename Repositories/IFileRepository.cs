using QLNhanSu.Models;

namespace QLNhanSu.Repositories
{
    public interface IFileRepository
    {
        Task<StoredFile> AddAsync(StoredFile file);
        Task<StoredFile?> GetByIdAsync(int id);
    }
}