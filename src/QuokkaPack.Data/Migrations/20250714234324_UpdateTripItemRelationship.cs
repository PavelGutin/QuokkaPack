using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTripItemRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripItem_Items_ItemId",
                table: "TripItem");

            migrationBuilder.DropForeignKey(
                name: "FK_TripItem_Trips_TripId",
                table: "TripItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripItem",
                table: "TripItem");

            migrationBuilder.RenameTable(
                name: "TripItem",
                newName: "TripItems");

            migrationBuilder.RenameIndex(
                name: "IX_TripItem_TripId",
                table: "TripItems",
                newName: "IX_TripItems_TripId");

            migrationBuilder.RenameIndex(
                name: "IX_TripItem_ItemId",
                table: "TripItems",
                newName: "IX_TripItems_ItemId");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "TripItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripItems",
                table: "TripItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripItems_Items_ItemId",
                table: "TripItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TripItems_Trips_TripId",
                table: "TripItems",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripItems_Items_ItemId",
                table: "TripItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TripItems_Trips_TripId",
                table: "TripItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripItems",
                table: "TripItems");

            migrationBuilder.RenameTable(
                name: "TripItems",
                newName: "TripItem");

            migrationBuilder.RenameIndex(
                name: "IX_TripItems_TripId",
                table: "TripItem",
                newName: "IX_TripItem_TripId");

            migrationBuilder.RenameIndex(
                name: "IX_TripItems_ItemId",
                table: "TripItem",
                newName: "IX_TripItem_ItemId");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "TripItem",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripItem",
                table: "TripItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripItem_Items_ItemId",
                table: "TripItem",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TripItem_Trips_TripId",
                table: "TripItem",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");
        }
    }
}
