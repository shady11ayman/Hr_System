using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class Deduction_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "ShiftTypes",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "ShiftTypes",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "ShiftTypeId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Deductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExitTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deductions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "empId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ShiftTypeId",
                table: "Employees",
                column: "ShiftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Deductions_EmployeeId",
                table: "Deductions",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ShiftTypes_ShiftTypeId",
                table: "Employees",
                column: "ShiftTypeId",
                principalTable: "ShiftTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ShiftTypes_ShiftTypeId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Deductions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ShiftTypeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ShiftTypes");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ShiftTypes");

            migrationBuilder.DropColumn(
                name: "ShiftTypeId",
                table: "Employees");
        }
    }
}
