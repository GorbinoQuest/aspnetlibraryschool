using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Data.Migrations
{
    /// <inheritdoc />
    public partial class BorrowingUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "BookBorrowings",
                type: "TEXT",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
