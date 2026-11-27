using System.Security.Claims;
using Microsoft.Data.SqlClient;
using QLNhanSu.Exceptions;
using QLNhanSu.Repositories;

namespace QLNhanSu.Services;

public class CurrentUserService(IUserRepository users, IEmployeeRepository employees, IHttpContextAccessor accessor)
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public async Task<(int Id, string Role)> GetAsync()
    {
        var principal = Principal;
        if (principal?.Identity?.IsAuthenticated != true ||
            !int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            throw new UnauthorizedException("Chưa xác thực tài khoản.");
        var user = await users.GetByIdAsync(id);
        if (user == null || !principal.IsInRole(user.Role))
            throw new UnauthorizedException("Tài khoản hoặc quyền đã thay đổi. Vui lòng đăng nhập lại.");
        if (user.Role is not ("Admin" or "HR" or "User"))
            throw new AppException("Không đủ quyền.", 403);
        return (user.Id, user.Role);
    }

    public async Task<int> EmployeeIdAsync(int userId)
        => await employees.GetActiveEmployeeIdByUserIdAsync(userId)
            ?? throw new AppException("Tài khoản chưa liên kết nhân viên đang hoạt động.", 403);

    public Task LinkAsync(int employeeId, int userId)
        => ChangeLinkAsync(employeeId, null, userId);

    public Task UnlinkAsync(int employeeId, int expectedUserId)
        => ChangeLinkAsync(employeeId, expectedUserId, null);

    public Task ReplaceLinkAsync(int employeeId, int expectedUserId, int newUserId)
        => ChangeLinkAsync(employeeId, expectedUserId, newUserId);

    private async Task ChangeLinkAsync(int employeeId, int? expectedUserId, int? newUserId)
    {
        var actor = await GetAsync();
        if (actor.Role != "Admin") throw new AppException("Không đủ quyền.", 403);
        var employee = await employees.GetByIdAsync(employeeId)
            ?? throw new NotFoundException("Không tìm thấy nhân viên.");
        if (newUserId.HasValue && !employee.IsActive)
            throw new BadRequestException("Nhân viên không hoạt động.");
        if (employee.UserId == newUserId && newUserId.HasValue &&
            (expectedUserId == null || expectedUserId == newUserId)) return;
        if (employee.UserId != expectedUserId)
            throw new AppException("Liên kết hiện tại không đúng hoặc đã thay đổi.", 409);
        if (newUserId.HasValue)
        {
            if (await users.GetByIdAsync(newUserId.Value) == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");
            if (await employees.IsUserLinkedAsync(newUserId.Value))
                throw new AppException("Tài khoản đã liên kết với nhân viên khác.", 409);
        }
        try
        {
            if (!await employees.ChangeUserLinkAsync(employeeId, expectedUserId, newUserId))
                throw new AppException("Liên kết vừa thay đổi. Vui lòng tải lại dữ liệu.", 409);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627 or 547)
        {
            throw new AppException("Không thể lưu liên kết do dữ liệu vừa thay đổi.", 409);
        }
    }
}
