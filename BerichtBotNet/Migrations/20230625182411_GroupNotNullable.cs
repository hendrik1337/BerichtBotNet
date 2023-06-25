using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerichtBotNet.Migrations
{
    /// <inheritdoc />
    public partial class GroupNotNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apprentices_Groups_GroupId",
                table: "Apprentices");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Apprentices",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Apprentices_Groups_GroupId",
                table: "Apprentices",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apprentices_Groups_GroupId",
                table: "Apprentices");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Apprentices",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Apprentices_Groups_GroupId",
                table: "Apprentices",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
