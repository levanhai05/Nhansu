using QLNhanSu.DTOs;
using QLNhanSu.Exceptions;
using QLNhanSu.Models;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _repo;

        public ContractService(IContractRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ContractDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(ToDto).ToList();
        }

        public async Task<List<ContractDto>> GetByEmployeeAsync(int employeeId)
        {
            var items = await _repo.GetByEmployeeIdAsync(employeeId);
            return items.Select(ToDto).ToList();
        }

        public async Task<ContractDto> CreateAsync(ContractCreateDto dto)
        {
            if (!await _repo.EmployeeExistsAsync(dto.EmployeeId))
                throw new BadRequestException("EmployeeId không tồn tại.");

            var contract = new Contract
            {
                EmployeeId = dto.EmployeeId,
                ContractType = dto.ContractType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Salary = dto.Salary,
                Status = "Active"
            };

            await _repo.AddAsync(contract);
            await _repo.SaveChangesAsync();

            // Nạp lại để có tên nhân viên trong DTO trả về
            var created = await _repo.GetByIdAsync(contract.Id);
            return ToDto(created!);
        }

        public async Task TerminateAsync(int id)
        {
            var contract = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy hợp đồng có Id = {id}.");

            contract.Status = "Terminated";
            await _repo.SaveChangesAsync();
        }

        private static ContractDto ToDto(Contract c) => new()
        {
            Id = c.Id,
            EmployeeId = c.EmployeeId,
            EmployeeName = c.Employee?.FullName,
            ContractType = c.ContractType,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Salary = c.Salary,
            Status = c.Status
        };
    }
}
