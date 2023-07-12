using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerichtBotNet.Migrations
{
    /// <inheritdoc />
    public partial class ApprenticeSkippedIsNowBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkipCount",
                table: "Apprentices");

            migrationBuilder.AddColumn<bool>(
                name: "Skipped",
                table: "Apprentices",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skipped",
                table: "Apprentices");

            migrationBuilder.AddColumn<int>(
                name: "SkipCount",
                table: "Apprentices",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
