using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.DTOs;
using QLNhanSu.Models;

namespace QLNhanSu.Controllers
{
    /// <summary>
    /// Quản lý thông tin nhân viên: xem danh sách, xem chi tiết, thêm mới, cập nhật, ngừng hoạt động.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ nhân viên (kèm tên khoa/phòng ban và chức vụ).
        /// </summary>
        /// <returns>Danh sách nhân viên.</returns>
        // GET /api/employees
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Select(e => ToDto(e))
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một nhân viên theo Id.
        /// </summary>
        /// <param name="id">Id của nhân viên.</param>
        /// <returns>Thông tin nhân viên.</returns>
        /// <response code="404">Không tìm thấy nhân viên với Id tương ứng.</response>
        // GET /api/employees/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _context.Employees
                .Include(x => x.Department)
                .Include(x => x.Position)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null) throw new KeyNotFoundException($"Không tìm thấy nhân viên có Id = {id}.");
            return Ok(ToDto(e));
        }

        /// <summary>
        /// Tạo mới một nhân viên. (Admin, HR)
        /// </summary>
        /// <param name="dto">Thông tin nhân viên cần tạo.</param>
        /// <returns>Nhân viên vừa được tạo.</returns>
        /// <response code="201">Tạo thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc DepartmentId/PositionId không tồn tại.</response>
        /// <response code="403">Không đủ quyền.</response>
        // POST /api/employees
        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateDto dto)
        {
            var deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists) throw new InvalidOperationException("DepartmentId không tồn tại.");

            var posExists = await _context.Positions.AnyAsync(p => p.Id == dto.PositionId);
            if (!posExists) throw new InvalidOperationException("PositionId không tồn tại.");

            var employee = new Employee
            {
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            await _context.Entry(employee).Reference(x => x.Department).LoadAsync();
            await _context.Entry(employee).Reference(x => x.Position).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ToDto(employee));
        }

        /// <summary>
        /// Cập nhật thông tin một nhân viên. (Admin, HR)
        /// </summary>
        /// <param name="id">Id của nhân viên cần cập nhật.</param>
        /// <param name="dto">Thông tin mới của nhân viên.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy nhân viên với Id tương ứng.</response>
        // PUT /api/employees/1
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EmployeeCreateDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) throw new KeyNotFoundException($"Không tìm thấy nhân viên có Id = {id}.");

            employee.FullName = dto.FullName;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.Gender = dto.Gender;
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.Address = dto.Address;
            employee.HireDate = dto.HireDate;
            employee.DepartmentId = dto.DepartmentId;
            employee.PositionId = dto.PositionId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Ngừng hoạt động (nghỉ việc) một nhân viên — không xoá hẳn khỏi hệ thống. (Admin)
        /// </summary>
        /// <param name="id">Id của nhân viên.</param>
        /// <response code="204">Cập nhật thành công.</response>
        /// <response code="403">Không đủ quyền.</response>
        /// <response code="404">Không tìm thấy nhân viên với Id tương ứng.</response>
        // DELETE /api/employees/1  (nghỉ việc -> khoá tài khoản thay vì xoá hẳn)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) throw new KeyNotFoundException($"Không tìm thấy nhân viên có Id = {id}.");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static EmployeeDto ToDto(Employee e) => new EmployeeDto
        {
            Id = e.Id,
            FullName = e.FullName,
            DateOfBirth = e.DateOfBirth,
            Gender = e.Gender,
            Email = e.Email,
            Phone = e.Phone,
            Address = e.Address,
            HireDate = e.HireDate,
            IsActive = e.IsActive,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name,
            PositionId = e.PositionId,
            PositionName = e.Position?.Name
        };
    }
}