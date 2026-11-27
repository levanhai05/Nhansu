using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QLNhanSu.Data;
using QLNhanSu.Exceptions;

namespace QLNhanSu.Services;

public class CurrentUserService(AppDbContext db, IHttpContextAccessor accessor)
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public async Task<(int Id, string Role)> GetAsync()
    {
        if (Principal?.Identity?.IsAuthenticated != true ||
            !int.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            throw new UnauthorizedException("Chưa xác thực tài khoản.");
        var user = await db.Users.AsNoTracking().Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Role }).SingleOrDefaultAsync();
        if (user == null || !Principal.IsInRole(user.Role))
            throw new UnauthorizedException("Tài khoản hoặc quyền đã thay đổi. Vui lòng đăng nhập lại.");
        if (user.Role is not ("Admin" or "HR" or "User"))
            throw new AppException("Không đủ quyền.", 403);
        return (user.Id, user.Role);
    }

    public async Task<int> EmployeeIdAsync(int userId)
    {
        var id = await db.Employees.AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .Select(e => (int?)e.Id).SingleOrDefaultAsync();
        return id ?? throw new AppException("Tài khoản chưa liên kết nhân viên đang hoạt động.", 403);
    }

    public async Task LinkAsync(int employeeId, int userId)
    {
        var actor = await GetAsync();
        if (actor.Role != "Admin") throw new AppException("Không đủ quyền.", 403);
        var employee = await db.Employees.FindAsync(employeeId)
            ?? throw new NotFoundException("Không tìm thấy nhân viên.");
        if (!employee.IsActive) throw new BadRequestException("Nhân viên không hoạt động.");
        if (!await db.Users.AnyAsync(u => u.Id == userId))
            throw new NotFoundException("Không tìm thấy tài khoản.");
        if (employee.UserId == userId) return;
        if (employee.UserId != null || await db.Employees.AnyAsync(e => e.UserId == userId))
            throw new AppException("Tài khoản hoặc nhân viên đã được liên kết.", 409);
        employee.UserId = userId;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { throw new AppException("Không thể lưu liên kết. Kiểm tra dữ liệu và thử lại.", 409); }
    }
}
