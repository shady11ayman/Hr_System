using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class shifttypeapplicationadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShiftTypeId",
                table: "EmployeeApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeApplications_ShiftTypeId",
                table: "EmployeeApplications",
                column: "ShiftTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeApplications_ShiftTypes_ShiftTypeId",
                table: "EmployeeApplications",
                column: "ShiftTypeId",
                principalTable: "ShiftTypes",
                principalColumn: "ShiftTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeApplications_ShiftTypes_ShiftTypeId",
                table: "EmployeeApplications");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeApplications_ShiftTypeId",
                table: "EmployeeApplications");

            migrationBuilder.DropColumn(
                name: "ShiftTypeId",
                table: "EmployeeApplications");
        }
    }
}
