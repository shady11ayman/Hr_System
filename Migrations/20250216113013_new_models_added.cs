using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class new_models_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deductions_Employees_EmployeeId",
                table: "Deductions");

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "salary",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "HrId",
                table: "Deductions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Deductions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isFinalized",
                table: "Deductions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "Deductions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hr_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    DepartmentdeptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkHours = table.Column<double>(type: "float", nullable: true),
                    ShiftTypeId = table.Column<int>(type: "int", nullable: true),
                    ContractTypeId = table.Column<int>(type: "int", nullable: true),
                    ContractStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.ManagerId);
                    table.ForeignKey(
                        name: "FK_Managers_ContractTypes_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalTable: "ContractTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Managers_Departments_DepartmentdeptId",
                        column: x => x.DepartmentdeptId,
                        principalTable: "Departments",
                        principalColumn: "deptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Managers_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Managers_ShiftTypes_ShiftTypeId",
                        column: x => x.ShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "ShiftTypeId");
                });

            migrationBuilder.CreateTable(
                name: "SalaryAfterDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    empId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeductionId = table.Column<int>(type: "int", nullable: false),
                    Salary = table.Column<double>(type: "float", nullable: false),
                    Salaryafterchange = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryAfterDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryAfterDeductions_Deductions_DeductionId",
                        column: x => x.DeductionId,
                        principalTable: "Deductions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalaryAfterDeductions_Employees_empId",
                        column: x => x.empId,
                        principalTable: "Employees",
                        principalColumn: "empId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deductions_ManagerId",
                table: "Deductions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_ContractTypeId",
                table: "Managers",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_DepartmentdeptId",
                table: "Managers",
                column: "DepartmentdeptId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_PositionId",
                table: "Managers",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_ShiftTypeId",
                table: "Managers",
                column: "ShiftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryAfterDeductions_DeductionId",
                table: "SalaryAfterDeductions",
                column: "DeductionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryAfterDeductions_empId",
                table: "SalaryAfterDeductions",
                column: "empId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deductions_Employees_EmployeeId",
                table: "Deductions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "empId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deductions_Managers_ManagerId",
                table: "Deductions",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Managers_ManagerId",
                table: "Employees",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "ManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deductions_Employees_EmployeeId",
                table: "Deductions");

            migrationBuilder.DropForeignKey(
                name: "FK_Deductions_Managers_ManagerId",
                table: "Deductions");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Managers_ManagerId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.DropTable(
                name: "SalaryAfterDeductions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Deductions_ManagerId",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "salary",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HrId",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "isFinalized",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "state",
                table: "Deductions");

            migrationBuilder.AddForeignKey(
                name: "FK_Deductions_Employees_EmployeeId",
                table: "Deductions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "empId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
