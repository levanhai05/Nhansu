using Microsoft.AspNetCore.Mvc;
using NhansuFE.Models;
using NhansuFE.Services;
namespace NhansuFE.Controllers;

[Roles]
public class DepartmentsController(ApiClient a) : ResourceController<Department>(a) {
  protected override string Endpoint => "/api/departments"; protected override string Title => "Khoa / Phòng ban";
}

[Roles]
public class PositionsController(ApiClient a) : ResourceController<Position>(a) {
  protected override string Endpoint => "/api/positions"; protected override string Title => "Chức vụ";
}

[Roles("Admin", "HR")]
public class EmployeesController(ApiClient a) : ResourceController<Employee>(a) {
  protected override string Endpoint => "/api/employees"; protected override string Title => "Quản lý nhân viên";
  protected override bool Paged => true; protected override string DeleteLabel => "Ngừng hoạt động";
  protected override async Task<List<Filter>> FiltersAsync() {
    var dep = await Api.Get<List<Department>>("/api/departments");
    var pos = await Api.Get<List<Position>>("/api/positions");
    return new() {
      new("searchTerm", "Tìm theo tên / email"),
      new("departmentId", "Khoa / phòng ban", dep.Select(x => new Opt(x.Id.ToString(), x.Name)).ToList()),
      new("positionId", "Chức vụ", pos.Select(x => new Opt(x.Id.ToString(), x.Name)).ToList()),
      new("isActive", "Trạng thái", new() { new("true", "Đang làm việc"), new("false", "Đã ngừng") })
    };
  }
  [HttpPost]
  public async Task<IActionResult> Link(int id, int userId, string mode) {
    if (Role != "Admin") { TempData["Err"] = "Chỉ Admin được liên kết tài khoản."; return RedirectToAction("Index"); }
    if (mode == "unlink") await Api.Delete($"/api/employees/{id}/link-user/{userId}");
    else await Api.Put($"/api/employees/{id}/link-user/{userId}");
    TempData["Ok"] = "Đã cập nhật liên kết tài khoản.";
    return RedirectToAction("Edit", new { id });
  }
}

[Roles("Admin", "HR")]
public class ContractsController(ApiClient a) : ResourceController<Contract>(a) {
  protected override string Endpoint => "/api/contracts"; protected override string Title => "Hợp đồng lao động";
  protected override bool Paged => true; protected override bool AllowEdit => false;
  protected override string[] DeleteRoles => Array.Empty<string>();
  protected override List<RowAction> Actions => new() { new("Chấm dứt", "terminate", "btn-outline-danger", "Status", "Active") };
  protected override Task<List<Filter>> FiltersAsync() => Task.FromResult(new List<Filter> { new("employeeId", "Mã nhân viên (ID)") });
  protected override string ListPath() => int.TryParse(Request.Query["employeeId"], out var i) ? $"{Endpoint}/employee/{i}" : Endpoint;
  protected override Task DoAct(int id, string act) => Api.Put($"{Endpoint}/{id}/terminate");
}

[Roles("Admin", "HR", "User")]
public class LeaveRequestsController(ApiClient a) : ResourceController<LeaveRequest>(a) {
  protected override string Endpoint => "/api/leaverequests"; protected override string Title => "Đơn nghỉ phép";
  protected override bool Paged => true; protected override bool AllowEdit => false;
  protected override string[] WriteRoles => new[] { "Admin", "HR", "User" };
  protected override string[] DeleteRoles => Array.Empty<string>();
  protected override List<RowAction> Actions => new() {
    new("Duyệt", "Approved", "btn-success", "Status", "Pending"),
    new("Từ chối", "Rejected", "btn-outline-danger", "Status", "Pending") };
  protected override async Task BeforeSave(LeaveRequest m) => m.EmployeeId = (await Api.Get<Employee>("/api/employees/me")).Id;
  protected override Task DoAct(int id, string act) => Api.Put($"{Endpoint}/{id}/status", new { status = act });
}

[Roles("Admin", "HR")]
public class UsersController(ApiClient a) : ResourceController<AppUser>(a) {
  protected override string Endpoint => "/api/users"; protected override string Title => "Tài khoản người dùng";
  protected override bool Paged => true; protected override string CreateEndpoint => "/api/users/register";
  protected override string[] WriteRoles => new[] { "Admin" };
  protected override Task<List<Filter>> FiltersAsync() => Task.FromResult(new List<Filter> {
    new("searchTerm", "Tìm theo tên / email"),
    new("role", "Vai trò", new() { new("Admin", "Admin"), new("HR", "HR"), new("User", "User") }) });
}
