using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class shiftrm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the ShiftTypes table
            migrationBuilder.DropTable(
                name: "ShiftTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "ShiftTypes",
               columns: table => new
               {
                   Id = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_ShiftTypes", x => x.Id);
               });
        }
    }
}
