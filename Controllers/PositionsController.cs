using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.DTOs;
using QLNhanSu.Models;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý Chức vụ (Giảng viên, Trưởng khoa, Nhân viên văn phòng...).
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PositionsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách toàn bộ Chức vụ.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.Positions
                .Select(p => new PositionDto { Id = p.Id, Name = p.Name, BaseSalary = p.BaseSalary })
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>Lấy thông tin một Chức vụ theo Id.</summary>
        /// <param name="id">Id của Chức vụ.</param>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Positions.FindAsync(id);
            if (p == null) throw new KeyNotFoundException($"Không tìm thấy Chức vụ có Id = {id}.");
            return Ok(new PositionDto { Id = p.Id, Name = p.Name, BaseSalary = p.BaseSalary });
        }

        /// <summary>Tạo mới một Chức vụ.</summary>
        /// <param name="dto">Thông tin Chức vụ cần tạo.</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        [HttpPost]
        public async Task<IActionResult> Create(PositionCreateDto dto)
        {
            var p = new Position { Name = dto.Name, BaseSalary = dto.BaseSalary };
            _context.Positions.Add(p);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = p.Id },
                new PositionDto { Id = p.Id, Name = p.Name, BaseSalary = p.BaseSalary });
        }

        /// <summary>Cập nhật thông tin một Chức vụ.</summary>
        /// <param name="id">Id của Chức vụ cần cập nhật.</param>
        /// <param name="dto">Thông tin mới.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PositionCreateDto dto)
        {
            var p = await _context.Positions.FindAsync(id);
            if (p == null) throw new KeyNotFoundException($"Không tìm thấy Chức vụ có Id = {id}.");
            p.Name = dto.Name;
            p.BaseSalary = dto.BaseSalary;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Xoá một Chức vụ.</summary>
        /// <param name="id">Id của Chức vụ cần xoá.</param>
        /// <response code="204">Xoá thành công.</response>
        /// <response code="404">Không tìm thấy.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    var p = await _context.Positions.FindAsync(id);
    if (p == null) throw new KeyNotFoundException($"Không tìm thấy Chức vụ có Id = {id}.");

    if (await _context.Employees.AnyAsync(e => e.PositionId == id))
        throw new InvalidOperationException("Không thể xoá Chức vụ đang có nhân viên.");

    _context.Positions.Remove(p);
    await _context.SaveChangesAsync();
    return NoContent();
}
    }
}
