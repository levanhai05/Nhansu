using Microsoft.AspNetCore.Mvc;
using NhansuFE.Controllers;
using NhansuFE.Services;
var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews(o => {
  o.Filters.Add<ApiExceptionFilter>();
  o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
b.Services.AddDistributedMemoryCache();
b.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromHours(8); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; });
b.Services.AddHttpContextAccessor();
b.Services.AddHttpClient<ApiClient>(c => c.BaseAddress = new Uri(b.Configuration["Api:BaseUrl"]!))
  .ConfigurePrimaryHttpMessageHandler(() => {
    var h = new HttpClientHandler();
    if (b.Configuration.GetValue<bool>("Api:IgnoreSslErrors"))
      h.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return h;
  });
var app = b.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();

