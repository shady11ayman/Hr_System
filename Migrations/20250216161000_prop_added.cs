using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class prop_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_deptId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_ContractTypes_ContractTypeId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Departments_DepartmentdeptId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Positions_PositionId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_ShiftTypes_ShiftTypeId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_DepartmentdeptId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "DepartmentdeptId",
                table: "Managers");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Managers",
                newName: "Address");

            migrationBuilder.AlterColumn<int>(
                name: "ShiftTypeId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContractTypeId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DirectManagerId",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeaveTypeId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDays",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractDuration",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEnd",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStart",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ContractTypeId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LeaveTypeId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WorkHours",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDays",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "EmployeeApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EmployeeApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceRate",
                table: "EmployeeApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "EmployeeApplications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalInsuranceRate",
                table: "EmployeeApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "EmployeeApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "EmployeeApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_DirectManagerId",
                table: "Managers",
                column: "DirectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_LeaveTypeId",
                table: "Managers",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ContractTypeId",
                table: "Employees",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_LeaveTypeId",
                table: "Employees",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PositionId",
                table: "Employees",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeApplications_deptId",
                table: "EmployeeApplications",
                column: "deptId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeApplications_HrId",
                table: "EmployeeApplications",
                column: "HrId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeApplications_ManagerId",
                table: "EmployeeApplications",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId",
                unique: true,
                filter: "[ManagerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Managers_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "ManagerId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeApplications_Departments_deptId",
                table: "EmployeeApplications",
                column: "deptId",
                principalTable: "Departments",
                principalColumn: "deptId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeApplications_Managers_HrId",
                table: "EmployeeApplications",
                column: "HrId",
                principalTable: "Managers",
                principalColumn: "ManagerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeApplications_Managers_ManagerId",
                table: "EmployeeApplications",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ContractTypes_ContractTypeId",
                table: "Employees",
                column: "ContractTypeId",
                principalTable: "ContractTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_deptId",
                table: "Employees",
                column: "deptId",
                principalTable: "Departments",
                principalColumn: "deptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_LeaveTypes_LeaveTypeId",
                table: "Employees",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_ContractTypes_ContractTypeId",
                table: "Managers",
                column: "ContractTypeId",
                principalTable: "ContractTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_LeaveTypes_LeaveTypeId",
                table: "Managers",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Managers_DirectManagerId",
                table: "Managers",
                column: "DirectManagerId",
                principalTable: "Managers",
                principalColumn: "ManagerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Positions_PositionId",
                table: "Managers",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_ShiftTypes_ShiftTypeId",
                table: "Managers",
                column: "ShiftTypeId",
                principalTable: "ShiftTypes",
                principalColumn: "ShiftTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Managers_ManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeApplications_Departments_deptId",
                table: "EmployeeApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeApplications_Managers_HrId",
                table: "EmployeeApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeApplications_Managers_ManagerId",
                table: "EmployeeApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ContractTypes_ContractTypeId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_deptId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_LeaveTypes_LeaveTypeId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_ContractTypes_ContractTypeId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_LeaveTypes_LeaveTypeId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Managers_DirectManagerId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Positions_PositionId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_ShiftTypes_ShiftTypeId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_DirectManagerId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_LeaveTypeId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ContractTypeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_LeaveTypeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PositionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeApplications_deptId",
                table: "EmployeeApplications");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeApplications_HrId",
                table: "EmployeeApplications");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeApplications_ManagerId",
                table: "EmployeeApplications");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "DirectManagerId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ContractDuration",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ContractEnd",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ContractStart",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ContractTypeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkHours",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "InsuranceRate",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "MedicalInsuranceRate",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Managers",
                newName: "address");

            migrationBuilder.AlterColumn<int>(
                name: "ShiftTypeId",
                table: "Managers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "Managers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ContractTypeId",
                table: "Managers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentdeptId",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Managers_DepartmentdeptId",
                table: "Managers",
                column: "DepartmentdeptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_deptId",
                table: "Employees",
                column: "deptId",
                principalTable: "Departments",
                principalColumn: "deptId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_ContractTypes_ContractTypeId",
                table: "Managers",
                column: "ContractTypeId",
                principalTable: "ContractTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Departments_DepartmentdeptId",
                table: "Managers",
                column: "DepartmentdeptId",
                principalTable: "Departments",
                principalColumn: "deptId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Positions_PositionId",
                table: "Managers",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_ShiftTypes_ShiftTypeId",
                table: "Managers",
                column: "ShiftTypeId",
                principalTable: "ShiftTypes",
                principalColumn: "ShiftTypeId");
        }
    }
}
