using Microsoft.AspNetCore.Mvc;
using NhansuFE.Models;
using NhansuFE.Services;
namespace NhansuFE.Controllers;

[Roles]
public class HomeController(ApiClient api) : Controller {
  async Task<string> Try(Func<Task<int>> f) { try { return (await f()).ToString(); } catch (ApiException e) when (e.Status != 401) { return "—"; } }
  public async Task<IActionResult> Index() {
    var role = HttpContext.Session.GetString("role");
    var stats = new List<Stat>();
    if (role is "Admin" or "HR") {
      stats.Add(new("Nhân viên", await Try(async () => (await api.Get<Paged<Employee>>("/api/employees?pageSize=1")).TotalCount), "primary", "people", "/Employees"));
      stats.Add(new("Khoa / phòng ban", await Try(async () => (await api.Get<List<Department>>("/api/departments")).Count), "success", "building", "/Departments"));
      stats.Add(new("Chức vụ", await Try(async () => (await api.Get<List<Position>>("/api/positions")).Count), "info", "award", "/Positions"));
      stats.Add(new("Hợp đồng", await Try(async () => (await api.Get<Paged<Contract>>("/api/contracts?pageSize=1")).TotalCount), "secondary", "file-earmark-text", "/Contracts"));
      stats.Add(new("Đơn nghỉ chờ duyệt", await Try(async () => (await api.Get<Paged<LeaveRequest>>("/api/leaverequests?pageSize=50")).Items.Count(x => x.Status == "Pending")), "warning", "calendar-check", "/LeaveRequests"));
      stats.Add(new("Tài khoản", await Try(async () => (await api.Get<Paged<AppUser>>("/api/users?pageSize=1")).TotalCount), "dark", "person-gear", "/Users"));
    } else {
      stats.Add(new("Đơn nghỉ phép của tôi", await Try(async () => (await api.Get<Paged<LeaveRequest>>("/api/leaverequests?pageSize=1")).TotalCount), "warning", "calendar-check", "/LeaveRequests"));
      stats.Add(new("Khoa / phòng ban", await Try(async () => (await api.Get<List<Department>>("/api/departments")).Count), "success", "building", "/Departments"));
    }
    return View(stats);
  }
  public async Task<IActionResult> Profile() => View(await api.Get<Employee>("/api/employees/me"));
}

public class AccountController(ApiClient api) : Controller {
  public IActionResult Login() => HttpContext.Session.GetString("token") != null ? RedirectToAction("Index", "Home") : View();
  [HttpPost]
  public async Task<IActionResult> Login(string username, string password) {
    try { await api.Login(username, password); }
    catch (ApiException e) { ViewBag.Error = e.Message; return View(); }
    return RedirectToAction("Index", "Home");
  }
  [HttpPost] public async Task<IActionResult> Logout() { await api.Logout(); return RedirectToAction("Login"); }
}
