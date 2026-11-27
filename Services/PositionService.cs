using QLNhanSu.DTOs;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _repo;

        public PositionService(IPositionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<PositionDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(ToDto).ToList();
        }

        public async Task<PositionDto> GetByIdAsync(int id)
        {
            var position = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy Chức vụ có Id = {id}.");
            return ToDto(position);
        }

        public async Task<PositionDto> CreateAsync(PositionCreateDto dto)
        {
            var position = new Position
            {
                Name = dto.Name.Trim(),
                BaseSalary = dto.BaseSalary
            };

            await _repo.AddAsync(position);
            await _repo.SaveChangesAsync();
            return ToDto(position);
        }

        public async Task UpdateAsync(int id, PositionCreateDto dto)
        {
            var position = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy Chức vụ có Id = {id}.");

            position.Name = dto.Name.Trim();
            position.BaseSalary = dto.BaseSalary;
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var position = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy Chức vụ có Id = {id}.");

            if (await _repo.HasEmployeesAsync(id))
                throw new BadRequestException("Không thể xoá Chức vụ đang có nhân viên.");

            _repo.Remove(position);
            await _repo.SaveChangesAsync();
        }

        private static PositionDto ToDto(Position p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            BaseSalary = p.BaseSalary
        };
    }
}
