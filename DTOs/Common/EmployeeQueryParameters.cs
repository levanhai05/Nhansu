// Đặt tại: DTOs/Common/EmployeeQueryParameters.cs
namespace QLNhanSu.DTOs.Common
{
    /// <summary>Tham số phân trang, tìm kiếm, lọc và sắp xếp danh sách nhân viên.</summary>
    public class EmployeeQueryParameters
    {
        private const int MaxPageSize = 50;

        private int _pageNumber = 1;
        /// <summary>Trang hiện tại (mặc định 1).</summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        private int _pageSize = 10;
        /// <summary>Số dòng mỗi trang (mặc định 10, tối đa 50).</summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }

        /// <summary>Tìm theo họ tên hoặc email.</summary>
        public string? SearchTerm { get; set; }

        /// <summary>Lọc theo Khoa/Phòng ban.</summary>
        public int? DepartmentId { get; set; }

        /// <summary>Lọc theo Chức vụ.</summary>
        public int? PositionId { get; set; }

        /// <summary>Lọc theo trạng thái làm việc (true = đang làm, false = đã nghỉ).</summary>
        public bool? IsActive { get; set; }

        /// <summary>Sắp xếp theo: fullname (mặc định), email, hiredate.</summary>
        public string? SortBy { get; set; }

        /// <summary>true = giảm dần.</summary>
        public bool IsDescending { get; set; } = false;
    }
}