using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerichtBotNet.Migrations
{
    /// <inheritdoc />
    public partial class removedGroupReminderWeekdayChoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderWeekDay",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_GroupId",
                table: "Logs",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_Groups_GroupId",
                table: "Logs",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_Groups_GroupId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_GroupId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Logs");

            migrationBuilder.AddColumn<int>(
                name: "ReminderWeekDay",
                table: "Groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
