using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class first_turn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    deptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deptName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.deptId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    empId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    empName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    empEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    empPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hr_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.empId);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_deptId",
                        column: x => x.deptId,
                        principalTable: "Departments",
                        principalColumn: "deptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_deptId",
                table: "Employees",
                column: "deptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
