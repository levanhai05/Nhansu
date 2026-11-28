using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNhanSu.DTOs.Attendance;
using QLNhanSu.Services;

namespace QLNhanSu.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        // POST api/attendance/check-in
        [HttpPost("check-in")]
        public async Task<ActionResult<AttendanceDto>> CheckIn()
        {
            return Ok(await _service.CheckInAsync());
        }

        // POST api/attendance/check-out
        [HttpPost("check-out")]
        public async Task<ActionResult<AttendanceDto>> CheckOut()
        {
            return Ok(await _service.CheckOutAsync());
        }

        // GET api/attendance/me?year=2026&month=11
        [HttpGet("me")]
        public async Task<ActionResult<AttendanceSummaryDto>> GetMine(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            return Ok(await _service.GetMyMonthAsync(year, month));
        }

        // GET api/attendance/monthly?year=2026&month=11
        // Chỉ Admin và HR được xem.
        [Authorize(Policy = "AdminOrHR")]
        [HttpGet("monthly")]
        public async Task<ActionResult<AttendanceSummaryDto>> GetMonthly(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            return Ok(await _service.GetAllMonthAsync(year, month));
        }
    }
}
