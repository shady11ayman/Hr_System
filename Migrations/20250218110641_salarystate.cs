using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class salarystate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalaryStatementId",
                table: "Deductions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalaryStatements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    HrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryStatements_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "empId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deductions_SalaryStatementId",
                table: "Deductions",
                column: "SalaryStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStatements_EmployeeId",
                table: "SalaryStatements",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deductions_SalaryStatements_SalaryStatementId",
                table: "Deductions",
                column: "SalaryStatementId",
                principalTable: "SalaryStatements",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deductions_SalaryStatements_SalaryStatementId",
                table: "Deductions");

            migrationBuilder.DropTable(
                name: "SalaryStatements");

            migrationBuilder.DropIndex(
                name: "IX_Deductions_SalaryStatementId",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "SalaryStatementId",
                table: "Deductions");
        }
    }
}
