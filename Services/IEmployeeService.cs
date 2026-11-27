// Đặt tại: Services/IEmployeeService.cs
using QLNhanSu.DTOs;
using QLNhanSu.DTOs.Common;

namespace QLNhanSu.Services
{
    public interface IEmployeeService
    {
        Task<PagedResult<EmployeeDto>> GetAllAsync(EmployeeQueryParameters queryParams);
        Task<EmployeeDto> GetByIdAsync(int id);
        Task<EmployeeDto> CreateAsync(EmployeeCreateDto dto);
        Task UpdateAsync(int id, EmployeeCreateDto dto);
        Task DeactivateAsync(int id);
    }
}