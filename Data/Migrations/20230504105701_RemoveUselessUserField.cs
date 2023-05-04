using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUselessUserField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookBorrowings_AspNetUsers_ApplicationUserId",
                table: "BookBorrowings");

            migrationBuilder.DropIndex(
                name: "IX_BookBorrowings_ApplicationUserId",
                table: "BookBorrowings");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "BookBorrowings");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "BookBorrowings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BookBorrowings_ApplicationUserId",
                table: "BookBorrowings",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookBorrowings_AspNetUsers_ApplicationUserId",
                table: "BookBorrowings",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
