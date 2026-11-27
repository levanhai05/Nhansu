using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.DTOs;
using QLNhanSu.Models;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý Hợp đồng lao động của nhân viên.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContractsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách toàn bộ hợp đồng (kèm tên nhân viên).</summary>
        // GET /api/contracts
        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.Contracts
                .Include(c => c.Employee)
                .Select(c => ToDto(c))
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>Lấy danh sách hợp đồng của một nhân viên cụ thể.</summary>
        /// <param name="employeeId">Id của nhân viên.</param>
        // GET /api/contracts/employee/1  -> xem hợp đồng theo nhân viên
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var items = await _context.Contracts
                .Where(c => c.EmployeeId == employeeId)
                .Include(c => c.Employee)
                .Select(c => ToDto(c))
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>Tạo mới một hợp đồng lao động cho nhân viên. Trạng thái mặc định là "Active".</summary>
        /// <param name="dto">Thông tin hợp đồng cần tạo.</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc EmployeeId không tồn tại.</response>
        // POST /api/contracts
        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        public async Task<IActionResult> Create(ContractCreateDto dto)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId);
            if (!employeeExists) throw new InvalidOperationException("EmployeeId không tồn tại.");

            var contract = new Contract
            {
                EmployeeId = dto.EmployeeId,
                ContractType = dto.ContractType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Salary = dto.Salary,
                Status = "Active"
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            await _context.Entry(contract).Reference(c => c.Employee).LoadAsync();

            return CreatedAtAction(nameof(GetByEmployee), new { employeeId = contract.EmployeeId }, ToDto(contract));
        }

        /// <summary>Chấm dứt (Terminated) một hợp đồng lao động.</summary>
        /// <param name="id">Id của hợp đồng cần chấm dứt.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy hợp đồng.</response>
        // PUT /api/contracts/1/terminate  -> chấm dứt hợp đồng
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("{id}/terminate")]
        public async Task<IActionResult> Terminate(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) throw new KeyNotFoundException($"Không tìm thấy hợp đồng có Id = {id}.");

            contract.Status = "Terminated";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static ContractDto ToDto(Contract c) => new ContractDto
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