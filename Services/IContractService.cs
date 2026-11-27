using QLNhanSu.DTOs;

namespace QLNhanSu.Services
{
    public interface IContractService
    {
        Task<List<ContractDto>> GetAllAsync();
        Task<List<ContractDto>> GetByEmployeeAsync(int employeeId);
        Task<ContractDto> CreateAsync(ContractCreateDto dto);
        Task TerminateAsync(int id);
    }
}
