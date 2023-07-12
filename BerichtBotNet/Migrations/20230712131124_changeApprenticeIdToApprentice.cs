using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerichtBotNet.Migrations
{
    /// <inheritdoc />
    public partial class changeApprenticeIdToApprentice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Logs_ApprenticeId",
                table: "Logs",
                column: "ApprenticeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_Apprentices_ApprenticeId",
                table: "Logs",
                column: "ApprenticeId",
                principalTable: "Apprentices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_Apprentices_ApprenticeId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_ApprenticeId",
                table: "Logs");
        }
    }
}
