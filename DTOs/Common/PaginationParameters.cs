using System.ComponentModel.DataAnnotations;
namespace QLNhanSu.DTOs.Common;
public class PaginationParameters
{
    [Range(1, 1000000)] public int PageNumber { get; set; } = 1;
    [Range(1, 50)] public int PageSize { get; set; } = 10;
    public int Offset => checked((PageNumber - 1) * PageSize);
}
