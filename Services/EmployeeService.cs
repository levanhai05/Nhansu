// Đặt tại: Services/EmployeeService.cs
using QLNhanSu.DTOs;
using QLNhanSu.DTOs.Common;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<EmployeeDto>> GetAllAsync(EmployeeQueryParameters queryParams)
        {
            var paged = await _repo.GetPagedAsync(queryParams);

            return new PagedResult<EmployeeDto>(
                paged.Items.Select(ToDto).ToList(),
                paged.TotalCount,
                paged.PageNumber,
                paged.PageSize);
        }

        public async Task<EmployeeDto> GetByIdAsync(int id)
        {
            var employee = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy nhân viên có Id = {id}.");
            return ToDto(employee);
        }

        public async Task<EmployeeDto> CreateAsync(EmployeeCreateDto dto)
        {
            await ValidateReferencesAsync(dto);

            var employee = new Employee
            {
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                IsActive = true
            };

            await _repo.AddAsync(employee);
            await _repo.SaveChangesAsync();

            // Nạp lại để có tên Khoa/Chức vụ trong DTO trả về
            var created = await _repo.GetByIdAsync(employee.Id);
            return ToDto(created!);
        }

        public async Task UpdateAsync(int id, EmployeeCreateDto dto)
        {
            var employee = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy nhân viên có Id = {id}.");

            await ValidateReferencesAsync(dto); // Trước đây Update thiếu bước này -> lỗi FK 500

            employee.FullName = dto.FullName;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.Gender = dto.Gender;
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.Address = dto.Address;
            employee.HireDate = dto.HireDate;
            employee.DepartmentId = dto.DepartmentId;
            employee.PositionId = dto.PositionId;

            await _repo.SaveChangesAsync();
        }

        // Nghỉ việc = đặt IsActive = false, KHÔNG xoá hẳn (giữ hợp đồng, đơn nghỉ phép)
        public async Task DeactivateAsync(int id)
        {
            var employee = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy nhân viên có Id = {id}.");

            employee.IsActive = false;
            await _repo.SaveChangesAsync();
        }

        private async Task ValidateReferencesAsync(EmployeeCreateDto dto)
        {
            var today = DateTime.UtcNow.AddHours(7).Date;
            if (dto.DateOfBirth == default || dto.HireDate == default)
                throw new BadRequestException("Ngày sinh và ngày vào làm là bắt buộc.");

            if (dto.DateOfBirth.Date >= today)
                throw new BadRequestException("Ngày sinh phải trước ngày hiện tại.");

            if (dto.HireDate.Date <= dto.DateOfBirth.Date)
                throw new BadRequestException("Ngày vào làm phải sau ngày sinh.");

            if (dto.HireDate.Date > today)
                throw new BadRequestException("Ngày vào làm không được ở tương lai.");

            if (!await _repo.DepartmentExistsAsync(dto.DepartmentId))
                throw new BadRequestException("DepartmentId không tồn tại.");

            if (!await _repo.PositionExistsAsync(dto.PositionId))
                throw new BadRequestException("PositionId không tồn tại.");
        }

        private static EmployeeDto ToDto(Employee e) => new()
        {
            Id = e.Id,
            FullName = e.FullName,
            DateOfBirth = e.DateOfBirth,
            Gender = e.Gender,
            Email = e.Email,
            Phone = e.Phone,
            Address = e.Address,
            HireDate = e.HireDate,
            IsActive = e.IsActive,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name,
            PositionId = e.PositionId,
            PositionName = e.Position?.Name
        };
    }
}