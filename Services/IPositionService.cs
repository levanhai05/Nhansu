using QLNhanSu.DTOs;

namespace QLNhanSu.Services
{
    public interface IPositionService
    {
        Task<List<PositionDto>> GetAllAsync();
        Task<PositionDto> GetByIdAsync(int id);
        Task<PositionDto> CreateAsync(PositionCreateDto dto);
        Task UpdateAsync(int id, PositionCreateDto dto);
        Task DeleteAsync(int id);
    }
}
