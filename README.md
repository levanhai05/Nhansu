# QLNhanSu — Backend quản lý nhân sự

Đồ án môn Lập trình Web C# sử dụng ASP.NET Core Web API và SQL Server. Backend cung cấp REST API quản lý tài khoản, nhân viên, phòng ban, chức vụ, hợp đồng, nghỉ phép và ảnh.

## Công nghệ và kiến thức áp dụng

- .NET 8, C#, ASP.NET Core Web API, Entity Framework Core 8 (Code First, Migration), SQL Server.
- Controller – Service – Repository, Interface, Dependency Injection (Scoped), DTO, Data Annotations, LINQ, async/await.
- JWT Bearer, Role/Policy-based Authorization (Admin, HR, User), Refresh Token (hash, xoay vòng, hết hạn 7 ngày).
- Pagination, Searching, Filtering, Sorting; Options Pattern; Swagger/OpenAPI; Serilog; custom middleware xử lý ngoại lệ và ghi log.
- Upload ảnh: kiểm tra kích thước, định dạng và nội dung với ImageSharp nếu đã áp dụng bản sửa mục 6. ErrorsController giới hạn Development nếu đã áp dụng mục 7.

## Cấu trúc

```text
QLNhanSu/
├── Controllers/     # HTTP endpoints
├── Services/        # Nghiệp vụ và interfaces
├── Repositories/    # Truy vấn dữ liệu và interfaces
├── DTOs/           # Request, response, phân trang
├── Models/         # Entity
├── Data/           # AppDbContext
├── Migrations/     # Lịch sử schema
├── Middleware/     # Xử lý lỗi, logging
├── Exceptions/     # Ngoại lệ nghiệp vụ
├── Settings/       # Options Pattern
├── Attributes/     # DevelopmentOnly (nếu đã thêm)
├── Program.cs      # DI, cấu hình, pipeline
├── appsettings.json
├── QLNhanSu.csproj
├── README.md
└── QLNhanSu.postman_collection.json
```

Luồng: HTTP request → Middleware → Controller → Service → Repository → EF Core/SQL Server → HTTP response.

## Cài đặt và chạy local

Yêu cầu: .NET SDK 8 hoặc SDK tương thích net8.0, SQL Server đang chạy, EF CLI 8.x, Postman (tuỳ chọn). Mở PowerShell tại thư mục chứa `QLNhanSu.csproj`.

```powershell
dotnet restore
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=QLNhanSu;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "JwtSettings:SecretKey" "<KHOA_NGAU_NHIEN_RIENG_TOI_THIEU_32_KY_TU>"
```

**Quan trọng:** Connection string chỉ là ví dụ; dùng đúng instance và tên database hiện có, không đổi tên nếu muốn giữ dữ liệu. Thay khóa JWT bằng giá trị ngẫu nhiên riêng. Giữ cấu hình Issuer, Audience, ExpiryMinutes và FileSettings hợp lệ. Không commit bí mật, mật khẩu, token hoặc database thật. User Secrets chỉ phục vụ phát triển, không phải kho bí mật Production.

Seed tùy chọn cho database thử nghiệm:

```powershell
dotnet user-secrets set "SeedUsers:Enabled" "true"
dotnet user-secrets set "SeedUsers:AdminPassword" "<MAT_KHAU_ADMIN_RIENG>"
dotnet user-secrets set "SeedUsers:DemoPassword" "<MAT_KHAU_DEMO_RIENG>"
```

Hai mật khẩu phải khác nhau và ít nhất 12 ký tự. Seed chỉ trong Development, chỉ thêm tài khoản chưa tồn tại; không đặt lại mật khẩu hay quyền của tài khoản cũ. Sau khi tạo dữ liệu thử nghiệm có thể đặt Enabled thành false. Đăng nhập bằng mật khẩu thực tế đã lưu của tài khoản cũ và chủ động thay mật khẩu mặc định yếu.

```powershell
dotnet build
dotnet ef migrations list
dotnet ef database update
dotnet run
```

Nếu chưa có EF CLI: `dotnet tool install --global dotnet-ef --version "8.*"` (nếu đã có, kiểm tra `dotnet ef --version`). Với bản Refresh Token, nếu chưa tạo migration tương ứng, chạy `dotnet ef migrations add AddRefreshToken` một lần, xem nội dung migration rồi mới update. Sao lưu database trước khi cập nhật; Program.cs có tự gọi Migrate lúc khởi động. Không xóa database để sửa lỗi migration.

- Base URL: http://localhost:8080
- Swagger (Development): http://localhost:8080/swagger/index.html
- ImageSharp 4.1.4: Debug có thể build với cảnh báo giấy phép; cần license hợp lệ cho Release hoặc thay thư viện phù hợp trước khi publish. Hai cảnh báo decimal Salary/BaseSalary cần xử lý bằng cấu hình precision phù hợp và kiểm tra migration.

## API và quyền truy cập

JWT nghĩa là yêu cầu đăng nhập; Admin / HR là một trong hai role. Bảng dưới mô tả route từ mã nguồn ban đầu và bản vá Refresh Token. Policy tương ứng AdminOnly, AdminOrHR giữ nguyên quyền cũ; không khẳng định đã kiểm thử quyền sở hữu dữ liệu.

| Method | Endpoint | HTTP thành công / dự kiến | Quyền |
|---|---|---|---|
| POST | `/api/Auth/login` | 200 | Công khai |
| POST | `/api/Auth/register` | 200 | Công khai |
| POST | `/api/Auth/refresh` | 200 | Công khai |
| POST | `/api/Auth/logout` | 204 | Công khai |
| GET | `/api/Users` | 200 | Admin / HR |
| GET | `/api/Users/{{userId}}` | 200 | Admin / HR |
| POST | `/api/Users/register` | 201 | Admin |
| PUT | `/api/Users/{{userId}}` | 200 | Admin |
| DELETE | `/api/Users/{{userId}}` | 200 | Admin |
| GET | `/api/Departments` | 200 | JWT |
| GET | `/api/Departments/{{departmentId}}` | 200 | JWT |
| POST | `/api/Departments` | 201 | Admin / HR |
| PUT | `/api/Departments/{{departmentId}}` | 204 | Admin / HR |
| DELETE | `/api/Departments/{{departmentId}}` | 204 | Admin |
| GET | `/api/Positions` | 200 | JWT |
| GET | `/api/Positions/{{positionId}}` | 200 | JWT |
| POST | `/api/Positions` | 201 | Admin / HR |
| PUT | `/api/Positions/{{positionId}}` | 204 | Admin / HR |
| DELETE | `/api/Positions/{{positionId}}` | 204 | Admin |
| GET | `/api/Employees` | 200 | JWT |
| GET | `/api/Employees/{{employeeId}}` | 200 | JWT |
| POST | `/api/Employees` | 201 | Admin / HR |
| PUT | `/api/Employees/{{employeeId}}` | 204 | Admin / HR |
| DELETE | `/api/Employees/{{employeeId}}` | 204 | Admin |
| GET | `/api/Contracts` | 200 | Admin / HR |
| GET | `/api/Contracts/employee/{{employeeId}}` | 200 | Admin / HR |
| POST | `/api/Contracts` | 201 | Admin / HR |
| PUT | `/api/Contracts/{{contractId}}/terminate` | 204 | Admin / HR |
| GET | `/api/LeaveRequests` | 200 | JWT |
| POST | `/api/LeaveRequests` | 200 | JWT |
| PUT | `/api/LeaveRequests/{{leaveRequestId}}/status` | 204 | Admin / HR |
| POST | `/api/Files/upload` | 200 | JWT |
| GET | `/api/Files/{{fileId}}` | 200 | JWT |
| GET | `/api/Errors/bad-request` | 400 | Công khai |
| GET | `/api/Errors/not-found` | 404 | Công khai |
| GET | `/api/Errors/unauthorized` | 401 | Công khai |
| GET | `/api/Errors/server` | 500 | Công khai |

Các route Errors chỉ dùng Development; nếu đã thêm filter mục 7, action không thực thi ở Production. HTTP 401 là chưa xác thực/token không hợp lệ; 403 là thiếu quyền; 400 là dữ liệu không hợp lệ; 404 là không tìm thấy.

## Kiểm thử với Postman

1. Import `QLNhanSu.postman_collection.json` vào Postman. Mở tab Variables, điền giá trị local của `adminUsername`, `adminPassword`, `demoPassword`, kiểm tra `baseUrl`. Không chia sẻ giá trị bí mật.
2. Chạy **01 Auth → Đăng nhập**. Script tự lưu `token` vào `accessToken` và `refreshToken` vào biến tương ứng. Các request bảo vệ kế thừa Bearer token của collection.
3. Chạy GET Users để kiểm tra phân trang; PageNumber mặc định 1, PageSize mặc định 10, tối đa 50. Bật các query tùy chọn trong tab Params khi cần.
4. Tạo phòng ban → chức vụ → nhân viên → hợp đồng/đơn nghỉ phép. Các request tạo thành công tự lưu ID vào biến collection. Chỉ dùng ID dữ liệu mẫu cho cập nhật, xóa hoặc chấm dứt. Không chạy Collection Runner toàn bộ theo thứ tự mặc định vì có các request xóa và logout; xóa phòng ban/chức vụ sau khi không còn nhân viên tham chiếu.
5. Upload: chọn ảnh hợp lệ trong trường form-data `File`; tải lại bằng ID đã lưu. Kiểm tra thêm ảnh rỗng, quá dung lượng hoặc không đúng định dạng. Dung lượng theo FileSettings (mặc định 1 MB).
6. Refresh: gọi Auth/refresh, kiểm tra nhận cặp token mới; token refresh cũ phải bị từ chối. Logout chỉ thu hồi refresh token; access token đã cấp có thể còn hiệu lực tới hạn. Bản vá hỗ trợ một refresh token hoạt động mỗi tài khoản.
7. Kiểm tra quyền bằng tài khoản User/HR riêng (đổi biến đăng nhập), gọi endpoint AdminOnly và xác nhận 403. Các test script mặc định dành cho tài khoản Admin và luồng thành công, trừ nhóm Errors. Test lỗi phải điều chỉnh kỳ vọng tương ứng.
8. Xác nhận Seed không ghi đè: sửa mật khẩu/role tài khoản mẫu, restart trong Development khi Seed bật, kiểm tra không bị reset; trả lại quyền cũ sau test.

## Tình trạng và giới hạn

README và collection được lập từ Controllers(2).zip và bản vá Refresh Token đã gửi trong cuộc trao đổi; không có snapshot toàn bộ dự án mới nhất sau các sửa đổi trên máy người dùng. Các route và mẫu payload đã đối chiếu tĩnh, chưa chạy HTTP thực tế. Cần xác nhận lại bằng Swagger, nhất là migration Refresh Token, các policy và filter Development. Phân quyền theo chủ sở hữu nhân viên/đơn nghỉ phép, kiểm thử tự động và triển khai Release chưa được xác nhận. Không sử dụng dữ liệu thật trong Postman.
