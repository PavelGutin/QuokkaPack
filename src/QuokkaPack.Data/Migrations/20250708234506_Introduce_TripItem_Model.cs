using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class Introduce_TripItem_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemTrip");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Trips",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TripItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPacked = table.Column<bool>(type: "INTEGER", nullable: false),
                    TripId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripItem_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripItem_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ItemId",
                table: "Trips",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TripItem_ItemId",
                table: "TripItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TripItem_TripId",
                table: "TripItem",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Items_ItemId",
                table: "Trips",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Items_ItemId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "TripItem");

            migrationBuilder.DropIndex(
                name: "IX_Trips_ItemId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Trips");

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
    }
}
