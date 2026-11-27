using QLNhanSu.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs;
using QLNhanSu.Services;

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
        private readonly IContractService _contractService;

        public ContractsController(IContractService contractService)
        {
            _contractService = contractService;
        }

        /// <summary>Lấy danh sách toàn bộ hợp đồng (kèm tên nhân viên). (Admin, HR)</summary>
        // GET /api/contracts
        [Authorize(Policy = "AdminOrHR")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<ContractDto>>> GetAll([FromQuery] PaginationParameters page)
        {
            return Ok(await _contractService.GetAllAsync(page));
        }

        /// <summary>Lấy danh sách hợp đồng của một nhân viên cụ thể. (Admin, HR)</summary>
        /// <param name="employeeId">Id của nhân viên.</param>
        // GET /api/contracts/employee/1
        [Authorize(Policy = "AdminOrHR")]
        [HttpGet("employee/{employeeId:int}")]
        public async Task<ActionResult<PagedResult<ContractDto>>> GetByEmployee(int employeeId, [FromQuery] PaginationParameters page)
        {
            return Ok(await _contractService.GetByEmployeeAsync(employeeId, page));
        }

        /// <summary>Tạo mới một hợp đồng lao động cho nhân viên. Trạng thái mặc định là "Active". (Admin, HR)</summary>
        /// <param name="dto">Thông tin hợp đồng cần tạo.</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc EmployeeId không tồn tại.</response>
        // POST /api/contracts
        [Authorize(Policy = "AdminOrHR")]
        [HttpPost]
        public async Task<ActionResult<ContractDto>> Create(ContractCreateDto dto)
        {
            var created = await _contractService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByEmployee), new { employeeId = created.EmployeeId }, created);
        }

        /// <summary>Chấm dứt (Terminated) một hợp đồng lao động. (Admin, HR)</summary>
        /// <param name="id">Id của hợp đồng cần chấm dứt.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy hợp đồng.</response>
        // PUT /api/contracts/1/terminate
        [Authorize(Policy = "AdminOrHR")]
        [HttpPut("{id:int}/terminate")]
        public async Task<IActionResult> Terminate(int id)
        {
            await _contractService.TerminateAsync(id);
            return NoContent();
        }
    }
}
