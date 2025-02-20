using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class ipaddress_added_inScanrecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ipAddress",
                table: "ScanRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ipAddress",
                table: "ScanRecords");
        }
    }
}
