using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTripCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripItems_Trips_TripId",
                table: "TripItems");

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
                name: "FK_TripItems_Trips_TripId",
                table: "TripItems");

            migrationBuilder.AddForeignKey(
                name: "FK_TripItems_Trips_TripId",
                table: "TripItems",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
