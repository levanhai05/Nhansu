using QLNhanSu.DTOs.Common;
using QLNhanSu.DTOs;

namespace QLNhanSu.Services
{
    public interface IContractService
    {
        Task<PagedResult<ContractDto>> GetAllAsync(PaginationParameters page);
        Task<PagedResult<ContractDto>> GetByEmployeeAsync(int employeeId, PaginationParameters page);
        Task<ContractDto> CreateAsync(ContractCreateDto dto);
        Task TerminateAsync(int id);
    }
}
