using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using NhansuFE.Models;
namespace NhansuFE.Services;

public class ApiException(int status, string message) : Exception(message) { public int Status { get; } = status; }

public class ApiClient(HttpClient http, IHttpContextAccessor ctx) {
  static readonly JsonSerializerOptions J = new(JsonSerializerDefaults.Web);
  ISession S => ctx.HttpContext!.Session;

  async Task<HttpResponseMessage> Go(HttpRequestMessage r) {
    try { return await http.SendAsync(r); }
    catch (HttpRequestException ex) { throw new ApiException(0, $"Không kết nối được BE ({http.BaseAddress}): {ex.Message}"); }
  }
  async Task<HttpResponseMessage> Send(HttpMethod m, string url, object? body = null, bool auth = true) {
    HttpRequestMessage Make() {
      var r = new HttpRequestMessage(m, url);
      var t = S.GetString("token");
      if (auth && !string.IsNullOrEmpty(t)) r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", t);
      if (body != null) r.Content = JsonContent.Create(body, body.GetType(), null, J);
      return r;
    }
    var res = await Go(Make());
    if (res.StatusCode == HttpStatusCode.Unauthorized && auth && await Refresh()) res = await Go(Make());
    if (res.IsSuccessStatusCode) return res;
    throw new ApiException((int)res.StatusCode, await Err(res));
  }
  async Task<bool> Refresh() {
    var rt = S.GetString("refresh");
    if (string.IsNullOrEmpty(rt)) return false;
    var r = await Go(new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh") { Content = JsonContent.Create(new { refreshToken = rt }, null, J) });
    if (!r.IsSuccessStatusCode) return false;
    Store((await r.Content.ReadFromJsonAsync<Tokens>(J))!);
    return true;
  }
  void Store(Tokens t) { S.SetString("token", t.Token); S.SetString("refresh", t.RefreshToken); S.SetString("role", RoleOf(t.Token)); }
  static string RoleOf(string jwt) {
    var p = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
    p = p.PadRight(p.Length + (4 - p.Length % 4) % 4, '=');
    using var d = JsonDocument.Parse(Convert.FromBase64String(p));
    foreach (var c in d.RootElement.EnumerateObject())
      if (c.Name.EndsWith("role", StringComparison.OrdinalIgnoreCase))
        return c.Value.ValueKind == JsonValueKind.Array ? c.Value[0].GetString()! : c.Value.GetString()!;
    return "User";
  }
  static async Task<string> Err(HttpResponseMessage r) {
    var s = await r.Content.ReadAsStringAsync();
    try {
      using var d = JsonDocument.Parse(s); var e = d.RootElement;
      if (e.ValueKind == JsonValueKind.Object) {
        if (e.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) return m.GetString()!;
        if (e.TryGetProperty("errors", out var er) && er.ValueKind == JsonValueKind.Object)
          return string.Join(" ", er.EnumerateObject().SelectMany(p => p.Value.EnumerateArray().Select(x => x.GetString())));
        if (e.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String) return t.GetString()!;
      }
    } catch { }
    return r.StatusCode == HttpStatusCode.Forbidden ? "Bạn không có quyền thực hiện thao tác này." : $"Lỗi từ BE ({(int)r.StatusCode}).";
  }

  public async Task<T> Get<T>(string url) => (await (await Send(HttpMethod.Get, url)).Content.ReadFromJsonAsync<T>(J))!;
  public Task Post(string url, object? body) => Send(HttpMethod.Post, url, body);
  public Task Put(string url, object? body = null) => Send(HttpMethod.Put, url, body);
  public Task Delete(string url) => Send(HttpMethod.Delete, url);
  public async Task Login(string user, string pass) {
    var r = await Send(HttpMethod.Post, "/api/auth/login", new { username = user, password = pass }, false);
    Store((await r.Content.ReadFromJsonAsync<Tokens>(J))!);
    S.SetString("user", user);
  }
  public async Task Logout() {
    var rt = S.GetString("refresh");
    try { if (!string.IsNullOrEmpty(rt)) await Send(HttpMethod.Post, "/api/auth/logout", new { refreshToken = rt }, false); } catch { }
    S.Clear();
  }
}
