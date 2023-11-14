using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerichtBotNet.Migrations
{
    /// <inheritdoc />
    public partial class groupDayOfWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReminderDayOfWeek",
                table: "Groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderDayOfWeek",
                table: "Groups");
        }
    }
}
