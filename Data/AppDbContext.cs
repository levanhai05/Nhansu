using Microsoft.EntityFrameworkCore;
using QLNhanSu.Models;

namespace QLNhanSu.Data
{
    // DbContext - cầu nối giữa code và database SQL Server
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<StoredFile> StoredFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Không cho xoá Khoa/Chức vụ khi còn nhân viên (tránh Cascade xoá dây chuyền)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Khoa/Phòng ban mẫu
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Khoa Công nghệ thông tin", Description = "Đào tạo ngành CNTT" },
                new Department { Id = 2, Name = "Phòng Tổ chức - Nhân sự", Description = "Quản lý nhân sự toàn trường" },
                new Department { Id = 3, Name = "Phòng Đào tạo", Description = "Quản lý chương trình đào tạo" }
            );

            // Seed Chức vụ mẫu
            modelBuilder.Entity<Position>().HasData(
                new Position { Id = 1, Name = "Giảng viên", BaseSalary = 12_000_000 },
                new Position { Id = 2, Name = "Trưởng khoa", BaseSalary = 20_000_000 },
                new Position { Id = 3, Name = "Chuyên viên nhân sự", BaseSalary = 10_000_000 }
            );

            // Seed Nhân viên mẫu
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    FullName = "Nguyễn Văn An",
                    DateOfBirth = new DateTime(1985, 5, 20),
                    Gender = "Nam",
                    Email = "an.nguyen@donga.edu.vn",
                    Phone = "0905123456",
                    Address = "Đà Nẵng",
                    HireDate = new DateTime(2015, 9, 1),
                    IsActive = true,
                    DepartmentId = 1,
                    PositionId = 2
                },
                new Employee
                {
                    Id = 2,
                    FullName = "Trần Thị Bích",
                    DateOfBirth = new DateTime(1992, 3, 15),
                    Gender = "Nữ",
                    Email = "bich.tran@donga.edu.vn",
                    Phone = "0905654321",
                    Address = "Đà Nẵng",
                    HireDate = new DateTime(2020, 2, 10),
                    IsActive = true,
                    DepartmentId = 2,
                    PositionId = 3
                }
            );
        }
    }
}