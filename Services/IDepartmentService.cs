// Đặt tại: Services/IDepartmentService.cs
using QLNhanSu.DTOs;

namespace QLNhanSu.Services
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto> GetByIdAsync(int id);
        Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto);
        Task UpdateAsync(int id, DepartmentCreateDto dto);
        Task DeleteAsync(int id);
    }
}