namespace QLNhanSu.DTOs.Common
{
    public class UserQueryParameters
    {
        private const int MaxPageSize = 50;

        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }

        // Search: Username, Email
        public string? SearchTerm { get; set; }

        // Filter
        public string? Role { get; set; }

        // Sort: "username", "email", "createdat"
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
    }
}