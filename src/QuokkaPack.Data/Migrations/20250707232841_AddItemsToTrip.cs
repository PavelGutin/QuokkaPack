using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemsToTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TripId",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_TripId",
                table: "Items",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Trips_TripId",
                table: "Items",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Trips_TripId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_TripId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TripId",
                table: "Items");
        }
    }
}
