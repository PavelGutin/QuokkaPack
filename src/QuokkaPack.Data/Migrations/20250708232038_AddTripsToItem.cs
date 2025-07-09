using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTripsToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "ItemTrip",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "INTEGER", nullable: false),
                    TripsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTrip", x => new { x.ItemsId, x.TripsId });
                    table.ForeignKey(
                        name: "FK_ItemTrip_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemTrip_Trips_TripsId",
                        column: x => x.TripsId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemTrip_TripsId",
                table: "ItemTrip",
                column: "TripsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemTrip");

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
    }
}
