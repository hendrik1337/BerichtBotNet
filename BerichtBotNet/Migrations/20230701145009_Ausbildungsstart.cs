using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerichtBotNet.Migrations
{
    /// <inheritdoc />
    public partial class Ausbildungsstart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartOfApprenticeship",
                table: "Groups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartOfApprenticeship",
                table: "Groups");
        }
    }
}
