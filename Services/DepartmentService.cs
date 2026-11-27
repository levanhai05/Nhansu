// Đặt tại: Services/DepartmentService.cs
using QLNhanSu.DTOs;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repo;

        public DepartmentService(IDepartmentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(ToDto).ToList();
        }

        public async Task<DepartmentDto> GetByIdAsync(int id)
        {
            var department = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy Khoa/Phòng ban có Id = {id}.");
            return ToDto(department);
        }

        public async Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto)
        {
            if (await _repo.NameExistsAsync(dto.Name))
                throw new BadRequestException("Tên Khoa/Phòng ban đã tồn tại.");

            var department = new Department
            {
                Name = dto.Name.Trim(),
                Description = dto.Description
            };

            await _repo.AddAsync(department);
            await _repo.SaveChangesAsync();
            return ToDto(department);
        }

        public async Task UpdateAsync(int id, DepartmentCreateDto dto)
        {
            var department = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy Khoa/Phòng ban có Id = {id}.");

            if (await _repo.NameExistsAsync(dto.Name, excludeId: id))
                throw new BadRequestException("Tên Khoa/Phòng ban đã tồn tại.");

            department.Name = dto.Name.Trim();
            department.Description = dto.Description;
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var department = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy Khoa/Phòng ban có Id = {id}.");

            if (await _repo.HasEmployeesAsync(id))
                throw new BadRequestException("Không thể xoá Khoa/Phòng ban đang có nhân viên.");

            _repo.Remove(department);
            await _repo.SaveChangesAsync();
        }

        private static DepartmentDto ToDto(Department d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description
        };
    }
}