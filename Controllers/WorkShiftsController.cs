using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs.WorkShifts;
using QLNhanSu.Services;

namespace QLNhanSu.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkShiftsController : ControllerBase
    {
        private readonly IWorkShiftService _service;
        public WorkShiftsController(IWorkShiftService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<WorkShiftDto>>> Get([FromQuery] bool includeInactive = false)
            => Ok(await _service.GetShiftsAsync(includeInactive));

        [HttpPost]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<ActionResult<WorkShiftDto>> Create(WorkShiftCreateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Update(int id, WorkShiftCreateDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpPut("{id:int}/deactivate")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _service.DeactivateAsync(id);
            return NoContent();
        }

        [HttpGet("assignments")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<ActionResult<List<EmployeeShiftAssignmentDto>>> Assignments([FromQuery] int? employeeId)
            => Ok(await _service.GetAssignmentsAsync(employeeId));

        [HttpPost("assignments")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Assign(AssignWorkShiftDto dto)
        {
            await _service.AssignAsync(dto);
            return Ok(new { message = "Phân ca thành công." });
        }

        [HttpGet("my-current")]
        public async Task<ActionResult<WorkShiftDto?>> MyCurrent()
            => Ok(await _service.GetMyCurrentShiftAsync());
    }
}
