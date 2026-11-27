using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhanSu.Models
{
    // Entity - đại diện Chức vụ (Giảng viên, Trưởng khoa, Nhân viên văn phòng...)
    public class Position
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản không được âm.")]
        public decimal BaseSalary { get; set; }                 // Lương cơ bản theo chức vụ

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
