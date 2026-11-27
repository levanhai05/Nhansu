using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkShiftAndEmployeeShiftAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShiftBreakEndTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShiftBreakStartTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShiftEndTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShiftGraceMinutes",
                table: "AttendanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShiftName",
                table: "AttendanceRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShiftStartTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkShiftId",
                table: "AttendanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    BreakEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    GraceMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkShifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShiftAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    WorkShiftId = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShiftAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftAssignments_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalTable: "WorkShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftAssignments_EmployeeId_EffectiveFrom",
                table: "EmployeeShiftAssignments",
                columns: new[] { "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftAssignments_WorkShiftId",
                table: "EmployeeShiftAssignments",
                column: "WorkShiftId");
            // WORKSHIFT_DEFAULT_SEED
            migrationBuilder.Sql(@"
DECLARE @ShiftId int;

SELECT TOP (1) @ShiftId = [Id]
FROM [WorkShifts]
WHERE [Name] = N'Hành chính'
  AND [StartTime] = '08:00:00'
  AND [EndTime] = '17:00:00';

IF @ShiftId IS NULL
BEGIN
    INSERT INTO [WorkShifts]
        ([Name], [StartTime], [EndTime],
         [BreakStartTime], [BreakEndTime],
         [GraceMinutes], [IsActive], [CreatedAt])
    VALUES
        (N'Hành chính', '08:00:00', '17:00:00',
         '12:00:00', '13:00:00',
         5, 1, SYSUTCDATETIME());

    SET @ShiftId = CONVERT(int, SCOPE_IDENTITY());
END;

INSERT INTO [EmployeeShiftAssignments]
    ([EmployeeId], [WorkShiftId], [EffectiveFrom], [EffectiveTo], [CreatedAt])
SELECT
    e.[Id], @ShiftId, '2026-11-28', NULL, SYSUTCDATETIME()
FROM [Employees] e
WHERE e.[IsActive] = 1
  AND NOT EXISTS (
      SELECT 1
      FROM [EmployeeShiftAssignments] a
      WHERE a.[EmployeeId] = e.[Id]
        AND a.[EffectiveFrom] <= '2026-11-28'
        AND (a.[EffectiveTo] IS NULL OR a.[EffectiveTo] >= '2026-11-28')
  );
");        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeShiftAssignments");

            migrationBuilder.DropTable(
                name: "WorkShifts");

            migrationBuilder.DropColumn(
                name: "ShiftBreakEndTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ShiftBreakStartTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ShiftEndTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ShiftGraceMinutes",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ShiftName",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ShiftStartTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "WorkShiftId",
                table: "AttendanceRecords");
        }
    }
}
