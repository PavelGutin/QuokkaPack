using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripItems_Trips_TripId",
                table: "TripItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Categories",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Categories");

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
