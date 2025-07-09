using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MasterUserId",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Items_MasterUserId",
                table: "Items",
                column: "MasterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_MasterUsers_MasterUserId",
                table: "Items",
                column: "MasterUserId",
                principalTable: "MasterUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_MasterUsers_MasterUserId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_MasterUserId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "MasterUserId",
                table: "Items");
        }
    }
}
