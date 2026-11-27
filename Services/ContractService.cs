using QLNhanSu.DTOs.Common;
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

        public async Task<PagedResult<ContractDto>> GetAllAsync(PaginationParameters page)
        {
            var items = await _repo.GetAllAsync(page);
            return new PagedResult<ContractDto>(items.Items.Select(ToDto).ToList(), items.TotalCount, items.PageNumber, items.PageSize);
        }

        public async Task<PagedResult<ContractDto>> GetByEmployeeAsync(int employeeId, PaginationParameters page)
        {
            var items = await _repo.GetAllAsync(page, employeeId);
            return new PagedResult<ContractDto>(items.Items.Select(ToDto).ToList(), items.TotalCount, items.PageNumber, items.PageSize);
        }

        public async Task<ContractDto> CreateAsync(ContractCreateDto dto)
        {
            if (!await _repo.EmployeeExistsAsync(dto.EmployeeId))
                throw new BadRequestException("EmployeeId không tồn tại.");

            if (dto.StartDate == default || dto.EndDate == DateTime.MinValue)
                throw new BadRequestException("Ngày hợp đồng không hợp lệ.");

            if (dto.EndDate.HasValue && dto.EndDate.Value.Date < dto.StartDate.Date)
                throw new BadRequestException("Ngày kết thúc không được trước ngày bắt đầu.");

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
