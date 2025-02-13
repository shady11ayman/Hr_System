using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class update_employeeapplication_deptid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeptId",
                table: "EmployeeApplications",
                newName: "deptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "deptId",
                table: "EmployeeApplications",
                newName: "DeptId");
        }
    }
}
