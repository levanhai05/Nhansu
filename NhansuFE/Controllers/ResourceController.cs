using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NhansuFE.Models;
using NhansuFE.Services;
namespace NhansuFE.Controllers;

public abstract class ResourceController<T> : Controller where T : class, new() {
  protected readonly ApiClient Api;
  protected ResourceController(ApiClient api) { Api = api; }
  protected abstract string Endpoint { get; }
  protected abstract string Title { get; }
  protected virtual bool Paged => false;
  protected virtual string CreateEndpoint => Endpoint;
  protected virtual bool AllowCreate => true;
  protected virtual bool AllowEdit => true;
  protected virtual string[] WriteRoles => new[] { "Admin", "HR" };
  protected virtual string[] DeleteRoles => new[] { "Admin" };
  protected virtual string[] ActionRoles => new[] { "Admin", "HR" };
  protected virtual string DeleteLabel => "Xóa";
  protected virtual List<RowAction> Actions => new();
  protected virtual Task<List<Filter>> FiltersAsync() => Task.FromResult(new List<Filter>());
  protected virtual string ListPath() => Endpoint;
  protected virtual Task BeforeSave(T m) => Task.CompletedTask;
  protected virtual Task DoAct(int id, string act) => Task.CompletedTask;

  protected string Role => HttpContext.Session.GetString("role") ?? "";
  const string FormView = "~/Views/Shared/Form.cshtml";

  public override void OnActionExecuting(ActionExecutingContext c) {
    ViewBag.Title = Title; ViewBag.Type = typeof(T);
    ViewBag.Ctl = (string)c.RouteData.Values["controller"]!;
    ViewBag.CanCreate = AllowCreate && WriteRoles.Contains(Role);
    ViewBag.CanEdit = AllowEdit && WriteRoles.Contains(Role);
    ViewBag.CanDelete = DeleteRoles.Contains(Role);
    ViewBag.CanAct = ActionRoles.Contains(Role);
    ViewBag.DeleteLabel = DeleteLabel; ViewBag.Actions = Actions;
    base.OnActionExecuting(c);
  }
  IActionResult? Deny(string[] roles) {
    if (roles.Contains(Role)) return null;
    TempData["Err"] = "Bạn không có quyền thực hiện thao tác này.";
    return RedirectToAction("Index");
  }

  public async Task<IActionResult> Index(int page = 1) {
    var filters = await FiltersAsync();
    if (!Paged) filters.Insert(0, new Filter("q", "Tìm kiếm"));
    ViewBag.Filters = filters;
    var vm = new ListVm { Page = page };
    if (Paged) {
      var q = new List<string> { $"pageNumber={page}", "pageSize=10" };
      foreach (var f in filters) { var v = Request.Query[f.Name].ToString(); if (!string.IsNullOrEmpty(v)) q.Add($"{f.Name}={Uri.EscapeDataString(v)}"); }
      var r = await Api.Get<Paged<T>>(ListPath() + "?" + string.Join("&", q));
      vm.Items = r.Items.Cast<object>().ToList(); vm.Total = r.TotalCount;
      vm.Pages = Math.Max(1, (int)Math.Ceiling(r.TotalCount / (double)(r.PageSize > 0 ? r.PageSize : 10)));
    } else {
      var all = await Api.Get<List<T>>(Endpoint);
      var s = Request.Query["q"].ToString();
      if (!string.IsNullOrWhiteSpace(s))
        all = all.Where(x => typeof(T).GetProperties().Any(p => (p.GetValue(x)?.ToString() ?? "").Contains(s, StringComparison.OrdinalIgnoreCase))).ToList();
      vm.Total = all.Count; vm.Pages = Math.Max(1, (int)Math.Ceiling(all.Count / 10.0));
      vm.Page = Math.Clamp(page, 1, vm.Pages);
      vm.Items = all.Skip((vm.Page - 1) * 10).Take(10).Cast<object>().ToList();
    }
    return View("~/Views/Shared/List.cshtml", vm);
  }

  public async Task<IActionResult> Edit(int id = 0) {
    if (Deny(WriteRoles) is { } d) return d;
    if (id == 0 ? !AllowCreate : !AllowEdit) return RedirectToAction("Index");
    var m = id == 0 ? new T() : await Api.Get<T>($"{Endpoint}/{id}");
    await FillLookups();
    return View(FormView, (object)m);
  }

  [HttpPost]
  public async Task<IActionResult> Save([FromForm] T m) {
    if (Deny(WriteRoles) is { } d) return d;
    var id = (int)(typeof(T).GetProperty("Id")!.GetValue(m) ?? 0);
    if (!ModelState.IsValid) {
      ViewBag.Error = "Vui lòng nhập đầy đủ và đúng định dạng các trường bắt buộc.";
      await FillLookups(); return View(FormView, (object)m);
    }
    try {
      await BeforeSave(m);
      if (id == 0) await Api.Post(CreateEndpoint, m); else await Api.Put($"{Endpoint}/{id}", m);
    } catch (ApiException e) when (e.Status != 401) {
      ViewBag.Error = e.Message; await FillLookups(); return View(FormView, (object)m);
    }
    TempData["Ok"] = "Đã lưu thành công.";
    return RedirectToAction("Index");
  }

  [HttpPost]
  public async Task<IActionResult> Delete(int id) {
    if (Deny(DeleteRoles) is { } d) return d;
    await Api.Delete($"{Endpoint}/{id}");
    TempData["Ok"] = "Đã thực hiện thành công.";
    return RedirectToAction("Index");
  }

  [HttpPost]
  public async Task<IActionResult> Act(int id, string act) {
    if (Deny(ActionRoles) is { } d) return d;
    await DoAct(id, act);
    TempData["Ok"] = "Đã cập nhật.";
    return RedirectToAction("Index");
  }

  protected async Task FillLookups() {
    var d = new Dictionary<string, List<Opt>>();
    foreach (var p in typeof(T).GetProperties()) {
      var r = p.GetCustomAttribute<RefAttribute>();
      if (r == null || d.ContainsKey(r.Name)) continue;
      d[r.Name] = r.Name switch {
        "departments" => (await Api.Get<List<Department>>("/api/departments")).Select(x => new Opt(x.Id.ToString(), x.Name)).ToList(),
        "positions" => (await Api.Get<List<Position>>("/api/positions")).Select(x => new Opt(x.Id.ToString(), x.Name)).ToList(),
        _ => (await Api.Get<Paged<Employee>>("/api/employees?pageSize=50")).Items.Select(x => new Opt(x.Id.ToString(), $"{x.FullName} (#{x.Id})")).ToList()
      };
    }
    ViewBag.Lookups = d;
  }
}
