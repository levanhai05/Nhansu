using Microsoft.AspNetCore.Mvc;
using NhansuFE.Models;
using NhansuFE.Services;

namespace NhansuFE.Controllers;

[Roles("Admin", "HR", "User")]
public class AttendancesController(ApiClient api) : Controller
{
    private readonly ApiClient _api = api;

    private static (int Year, int Month) VietnamNow()
    {
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch { zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
        return (now.Year, now.Month);
    }

    private static bool ValidMonth(int year, int month)
        => year >= 2000 && year <= 2100 && month >= 1 && month <= 12;

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now = VietnamNow();
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        if (!ValidMonth(y, m))
        {
            TempData["Err"] = "Tháng hoặc năm không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var data = await _api.Get<AttendanceSummaryVm>($"/api/attendance/me?year={y}&month={m}");
        data.Year = y;
        data.Month = m;

        ViewBag.Today = DateTime.UtcNow;
        ViewBag.IsHR = HttpContext.Session.GetString("role") is "Admin" or "HR";
        ViewBag.CurrentShift = await _api.Get<WorkShiftVm?>("/api/workshifts/my-current");
        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn()
    {
        try
        {
            await _api.Post("/api/attendance/check-in", null);
            TempData["Ok"] = "Chấm công vào thành công.";
        }
        catch (ApiException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut()
    {
        try
        {
            await _api.Post("/api/attendance/check-out", null);
            TempData["Ok"] = "Chấm công ra thành công.";
        }
        catch (ApiException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [Roles("Admin", "HR")]
    public async Task<IActionResult> Monthly(int? year, int? month)
    {
        var now = VietnamNow();
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        if (!ValidMonth(y, m))
        {
            TempData["Err"] = "Tháng hoặc năm không hợp lệ.";
            return RedirectToAction(nameof(Monthly));
        }

        var data = await _api.Get<AttendanceSummaryVm>($"/api/attendance/monthly?year={y}&month={m}");
        data.Year = y;
        data.Month = m;
        return View(data);
    }
}
