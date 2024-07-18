using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testingdatabase.Migrations
{
    /// <inheritdoc />
    public partial class addedusername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LeaveDate",
                table: "LeaveRequests",
                newName: "StartDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "LeaveRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "LeaveRequests");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "LeaveRequests",
                newName: "LeaveDate");
        }
    }
}
