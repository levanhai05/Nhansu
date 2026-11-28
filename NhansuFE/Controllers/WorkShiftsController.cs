using Microsoft.AspNetCore.Mvc;
using NhansuFE.Models;
using NhansuFE.Services;

namespace NhansuFE.Controllers;

[Roles("Admin", "HR")]
public class WorkShiftsController(ApiClient api) : Controller
{
    private readonly ApiClient _api = api;

    public async Task<IActionResult> Index()
    {
        ViewBag.Shifts = await _api.Get<List<WorkShiftVm>>("/api/workshifts?includeInactive=true");
        ViewBag.Assignments = await _api.Get<List<EmployeeShiftAssignmentVm>>("/api/workshifts/assignments");
        return View(new WorkShiftCreateVm
        {
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            BreakStartTime = new TimeSpan(12, 0, 0),
            BreakEndTime = new TimeSpan(13, 0, 0),
            GraceMinutes = 5
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkShiftCreateVm vm)
    {
        try
        {
            await _api.Post("/api/workshifts", vm);
            TempData["Ok"] = "Tạo ca làm việc thành công.";
        }
        catch (ApiException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var shifts = await _api.Get<List<WorkShiftVm>>("/api/workshifts?includeInactive=true");
        var item = shifts.FirstOrDefault(x => x.Id == id);
        if (item == null) return NotFound();

        return View(new WorkShiftCreateVm
        {
            Name = item.Name, StartTime = item.StartTime, EndTime = item.EndTime,
            BreakStartTime = item.BreakStartTime, BreakEndTime = item.BreakEndTime,
            GraceMinutes = item.GraceMinutes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkShiftCreateVm vm)
    {
        try
        {
            await _api.Put($"/api/workshifts/{id}", vm);
            TempData["Ok"] = "Cập nhật ca làm việc thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            TempData["Err"] = ex.Message;
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _api.Put($"/api/workshifts/{id}/deactivate");
            TempData["Ok"] = "Đã ngừng hoạt động ca làm việc.";
        }
        catch (ApiException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignWorkShiftVm vm)
    {
        try
        {
            await _api.Post("/api/workshifts/assignments", vm);
            TempData["Ok"] = "Phân ca cho nhân viên thành công.";
        }
        catch (ApiException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
