using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NhansuFE.Services;
namespace NhansuFE.Controllers;

/// <summary>Bắt buộc đăng nhập; nếu có roles thì chỉ cho các vai trò đó.</summary>
public class RolesAttribute(params string[] roles) : ActionFilterAttribute {
  public override void OnActionExecuting(ActionExecutingContext c) {
    var s = c.HttpContext.Session;
    if (s.GetString("token") == null) { c.Result = new RedirectToActionResult("Login", "Account", null); return; }
    if (roles.Length > 0 && !roles.Contains(s.GetString("role"))) {
      ((Controller)c.Controller).TempData["Err"] = "Bạn không có quyền truy cập chức năng này.";
      c.Result = new RedirectToActionResult("Index", "Home", null);
    }
  }
}

/// <summary>Chuyển lỗi gọi API thành thông báo thân thiện.</summary>
public class ApiExceptionFilter(ITempDataDictionaryFactory tempFactory) : IExceptionFilter {
  public void OnException(ExceptionContext c) {
    if (c.Exception is not ApiException e) return;
    c.ExceptionHandled = true;
    var temp = tempFactory.GetTempData(c.HttpContext); var req = c.HttpContext.Request;
    if (e.Status == 401) {
      c.HttpContext.Session.Clear(); temp["Err"] = "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.";
      c.Result = new RedirectToActionResult("Login", "Account", null); return;
    }
    temp["Err"] = e.Message;
    var p = req.Path.Value ?? "/";
    if (p == "/" || p.Equals("/Home", StringComparison.OrdinalIgnoreCase) || p.Equals("/Home/Index", StringComparison.OrdinalIgnoreCase))
      c.Result = new ContentResult { Content = "Lỗi: " + e.Message, ContentType = "text/plain; charset=utf-8" };
    else {
      var back = req.Headers.Referer.ToString();
      c.Result = new RedirectResult(HttpMethods.IsGet(req.Method) || string.IsNullOrEmpty(back) ? "/" : back);
    }
  }
}
