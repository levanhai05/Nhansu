# NhansuFE – Giao diện quản lý nhân sự Đại học Đông Á (ASP.NET Core 8 MVC)
FE gọi trực tiếp API của BE QLNhanSu (JWT). FE gọi API từ phía server nên KHÔNG cần cấu hình CORS.

## Chạy
1. Cài .NET 8 SDK. Tin cậy chứng chỉ dev: `dotnet dev-certs https --trust`
2. Chạy BE (QLNhanSu). Xem địa chỉ HTTPS của BE trong BE/Properties/launchSettings.json.
3. Sửa `Api:BaseUrl` trong `appsettings.json` cho đúng (mặc định https://localhost:7003).
4. Trong thư mục NhansuFE: `dotnet run` rồi mở http://localhost:5100 và đăng nhập bằng tài khoản có sẵn trong CSDL.

## Chức năng theo vai trò
- Admin: toàn quyền (nhân viên, khoa/phòng ban, chức vụ, hợp đồng, nghỉ phép, tài khoản, liên kết tài khoản – nhân viên).
- HR: xem/sửa nhân viên, khoa/phòng ban, chức vụ, hợp đồng, duyệt đơn nghỉ phép, xem tài khoản.
- User: xem hồ sơ cá nhân, gửi và xem đơn nghỉ phép của mình (cần Admin liên kết tài khoản với nhân viên trước).
Tự động làm mới token (refresh) khi hết hạn; hết phiên thì quay lại trang đăng nhập.

## Ghi chú
- Gửi đơn nghỉ phép: BE yêu cầu tài khoản đã liên kết nhân viên đang hoạt động.
- Chức năng Files (upload ảnh) của BE chưa dùng vì chưa gắn với hồ sơ nhân viên.
