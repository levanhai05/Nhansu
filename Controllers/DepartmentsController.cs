using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.DTOs;
using QLNhanSu.Models;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý Khoa/Phòng ban trong trường.
    /// </summary>
    [Authorize]
[Route("api/[controller]")]
[ApiController]
    public class DepartmentsController : ControllerBase
    
    {
        private readonly AppDbContext _context;

        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách toàn bộ Khoa/Phòng ban.</summary>
        // GET /api/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.Departments
                .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, Description = d.Description })
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>Lấy thông tin một Khoa/Phòng ban theo Id.</summary>
        /// <param name="id">Id của Khoa/Phòng ban.</param>
        /// <response code="404">Không tìm thấy.</response>
        // GET /api/departments/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await _context.Departments.FindAsync(id);
            if (d == null) throw new KeyNotFoundException($"Không tìm thấy Khoa/Phòng ban có Id = {id}.");
            return Ok(new DepartmentDto { Id = d.Id, Name = d.Name, Description = d.Description });
        }

        /// <summary>Tạo mới một Khoa/Phòng ban.</summary>
        /// <param name="dto">Thông tin Khoa/Phòng ban cần tạo.</param>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ.</response>
        // POST /api/departments
        [HttpPost]
        public async Task<IActionResult> Create(DepartmentCreateDto dto)
        {
            var d = new Department { Name = dto.Name, Description = dto.Description };
            _context.Departments.Add(d);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = d.Id },
                new DepartmentDto { Id = d.Id, Name = d.Name, Description = d.Description });
        }

        /// <summary>Cập nhật thông tin một Khoa/Phòng ban.</summary>
        /// <param name="id">Id của Khoa/Phòng ban cần cập nhật.</param>
        /// <param name="dto">Thông tin mới.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="404">Không tìm thấy.</response>
        // PUT /api/departments/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DepartmentCreateDto dto)
        {
            var d = await _context.Departments.FindAsync(id);
            if (d == null) throw new KeyNotFoundException($"Không tìm thấy Khoa/Phòng ban có Id = {id}.");
            d.Name = dto.Name;
            d.Description = dto.Description;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Xoá một Khoa/Phòng ban.</summary>
        /// <param name="id">Id của Khoa/Phòng ban cần xoá.</param>
        /// <response code="204">Xoá thành công.</response>
        /// <response code="404">Không tìm thấy.</response>
        // DELETE /api/departments/1
        [HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    var d = await _context.Departments.FindAsync(id);
    if (d == null) throw new KeyNotFoundException($"Không tìm thấy Khoa/Phòng ban có Id = {id}.");

    if (await _context.Employees.AnyAsync(e => e.DepartmentId == id))
        throw new InvalidOperationException("Không thể xoá Khoa/Phòng ban đang có nhân viên.");

    _context.Departments.Remove(d);
    await _context.SaveChangesAsync();
    return NoContent();
}
    }
}
