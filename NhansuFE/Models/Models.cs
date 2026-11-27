using System.ComponentModel.DataAnnotations;
namespace NhansuFE.Models;

[AttributeUsage(AttributeTargets.Property)] public class NoFormAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property)] public class NoListAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property)] public class SecretAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property)] public class RefAttribute(string name) : Attribute { public string Name { get; } = name; }
[AttributeUsage(AttributeTargets.Property)] public class ChoicesAttribute(params string[] values) : Attribute { public string[] Values { get; } = values; }

public record Opt(string Value, string Text);
public record Filter(string Name, string Label, List<Opt>? Options = null);
public record RowAction(string Label, string Action, string Css, string? WhenProp = null, string? WhenValue = null);
public record Tokens(string Token, string RefreshToken);
public class Paged<T> { public List<T> Items { get; set; } = new(); public int PageNumber { get; set; } public int PageSize { get; set; } public int TotalCount { get; set; } }
public class ListVm { public List<object> Items { get; set; } = new(); public int Page { get; set; } = 1; public int Pages { get; set; } = 1; public int Total { get; set; } }
public record Stat(string Label, string Value, string Css, string Icon, string Link);

public class Department {
  [Display(Name = "ID")] public int Id { get; set; }
  [Required(ErrorMessage = "Nhập tên")] [Display(Name = "Tên khoa / phòng ban")] public string Name { get; set; } = "";
  [DisplayFormat(ConvertEmptyStringToNull = false)] [Display(Name = "Mô tả")] public string? Description { get; set; }
}
public class Position {
  [Display(Name = "ID")] public int Id { get; set; }
  [Required(ErrorMessage = "Nhập tên")] [Display(Name = "Tên chức vụ")] public string Name { get; set; } = "";
  [Display(Name = "Lương cơ bản (VNĐ)")] public decimal BaseSalary { get; set; }
}
public class Employee {
  [Display(Name = "ID")] public int Id { get; set; }
  [Required] [Display(Name = "Họ tên")] public string FullName { get; set; } = "";
  [Display(Name = "Ngày sinh")] public DateTime DateOfBirth { get; set; }
  [Required] [Choices("Nam", "Nữ", "Khác")] [Display(Name = "Giới tính")] public string Gender { get; set; } = "";
  [Required, EmailAddress] [Display(Name = "Email")] public string Email { get; set; } = "";
  [Required] [Display(Name = "Số điện thoại")] public string Phone { get; set; } = "";
  [DisplayFormat(ConvertEmptyStringToNull = false)] [NoList] [Display(Name = "Địa chỉ")] public string? Address { get; set; }
  [Display(Name = "Ngày vào làm")] public DateTime HireDate { get; set; }
  [NoForm] [Display(Name = "Đang làm việc")] public bool? IsActive { get; set; }
  [Ref("departments")] [NoList] [Display(Name = "Khoa / phòng ban")] public int DepartmentId { get; set; }
  [NoForm] [Display(Name = "Khoa / phòng ban")] public string? DepartmentName { get; set; }
  [Ref("positions")] [NoList] [Display(Name = "Chức vụ")] public int PositionId { get; set; }
  [NoForm] [Display(Name = "Chức vụ")] public string? PositionName { get; set; }
}
public class Contract {
  [Display(Name = "ID")] public int Id { get; set; }
  [Ref("employees")] [NoList] [Display(Name = "Nhân viên")] public int EmployeeId { get; set; }
  [NoForm] [Display(Name = "Nhân viên")] public string? EmployeeName { get; set; }
  [Required] [Choices("Thử việc", "Xác định thời hạn", "Không xác định thời hạn")] [Display(Name = "Loại hợp đồng")] public string ContractType { get; set; } = "";
  [Display(Name = "Ngày bắt đầu")] public DateTime StartDate { get; set; }
  [Display(Name = "Ngày kết thúc")] public DateTime? EndDate { get; set; }
  [Display(Name = "Lương (VNĐ)")] public decimal Salary { get; set; }
  [NoForm] [Display(Name = "Trạng thái")] public string? Status { get; set; }
}
public class LeaveRequest {
  [Display(Name = "ID")] public int Id { get; set; }
  [NoForm] [NoList] public int? EmployeeId { get; set; }
  [NoForm] [Display(Name = "Nhân viên")] public string? EmployeeName { get; set; }
  [Display(Name = "Từ ngày")] public DateTime StartDate { get; set; }
  [Display(Name = "Đến ngày")] public DateTime EndDate { get; set; }
  [Required] [Display(Name = "Lý do")] public string Reason { get; set; } = "";
  [NoForm] [Display(Name = "Trạng thái")] public string? Status { get; set; }
  [NoForm] [Display(Name = "Ngày gửi")] public DateTime? CreatedAt { get; set; }
}
public class AppUser {
  [Display(Name = "ID")] public int Id { get; set; }
  [Required] [Display(Name = "Tên đăng nhập (tối đa 20 ký tự)")] public string Username { get; set; } = "";
  [Required, EmailAddress] [Display(Name = "Email")] public string Email { get; set; } = "";
  [Required] [Choices("User", "HR", "Admin")] [Display(Name = "Vai trò")] public string Role { get; set; } = "";
  [Secret] [NoList] [Display(Name = "Mật khẩu (từ 12 ký tự; khi sửa để trống = giữ nguyên)")] public string? Password { get; set; }
  [NoForm] [Display(Name = "Ngày tạo")] public DateTime? CreatedAt { get; set; }
}
