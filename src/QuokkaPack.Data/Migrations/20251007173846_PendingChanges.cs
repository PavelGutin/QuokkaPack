using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterUsers_AspNetUsers_IdentityUserId",
                table: "MasterUsers");

            migrationBuilder.DropIndex(
                name: "IX_MasterUsers_IdentityUserId",
                table: "MasterUsers");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "MasterUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "MasterUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsers_IdentityUserId",
                table: "MasterUsers",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MasterUsers_AspNetUsers_IdentityUserId",
                table: "MasterUsers",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
