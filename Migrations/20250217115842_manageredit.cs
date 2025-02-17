using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hr_System_Demo_3.Migrations
{
    /// <inheritdoc />
    public partial class manageredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_Managers_ContractTypeId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_DirectManagerId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_LeaveTypeId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_PositionId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_ShiftTypeId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ContractEnd",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ContractStart",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ContractTypeId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "DirectManagerId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "Hr_Id",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ShiftTypeId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "WorkHours",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Managers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEnd",
                table: "Managers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStart",
                table: "Managers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractTypeId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DirectManagerId",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Hr_Id",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "LeaveTypeId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShiftTypeId",
                table: "Managers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WorkHours",
                table: "Managers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDays",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_ContractTypeId",
                table: "Managers",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_DirectManagerId",
                table: "Managers",
                column: "DirectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_LeaveTypeId",
                table: "Managers",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_PositionId",
                table: "Managers",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_ShiftTypeId",
                table: "Managers",
                column: "ShiftTypeId");

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
    }
}
