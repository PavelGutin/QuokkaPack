using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuokkaPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasterUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterUsers_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppUserLogins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Issuer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MasterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserLogins_MasterUsers_MasterUserId",
                        column: x => x.MasterUserId,
                        principalTable: "MasterUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    MasterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_MasterUsers_MasterUserId",
                        column: x => x.MasterUserId,
                        principalTable: "MasterUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MasterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_MasterUsers_MasterUserId",
                        column: x => x.MasterUserId,
                        principalTable: "MasterUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MasterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_MasterUsers_MasterUserId",
                        column: x => x.MasterUserId,
                        principalTable: "MasterUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    IsPacked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripItems_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MasterUsers",
                columns: new[] { "Id", "CreatedAt", "IdentityUserId" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsDefault", "MasterUserId", "Name" },
                values: new object[,]
                {
                    { 1, false, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Toiletries" },
                    { 2, false, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Clothing" },
                    { 3, false, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Electronics" },
                    { 4, false, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Outdoor Gear" },
                    { 5, false, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Snacks" },
                    { 6, false, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Documents" }
                });

            migrationBuilder.InsertData(
                table: "Trips",
                columns: new[] { "Id", "Destination", "EndDate", "MasterUserId", "StartDate" },
                values: new object[,]
                {
                    { 1, "Tokyo", new DateOnly(2025, 4, 24), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateOnly(2025, 4, 10) },
                    { 2, "Yosemite", new DateOnly(2025, 6, 8), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateOnly(2025, 6, 1) },
                    { 3, "Paris", new DateOnly(2025, 7, 30), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateOnly(2025, 7, 15) },
                    { 4, "Banff", new DateOnly(2025, 9, 20), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateOnly(2025, 9, 10) },
                    { 5, "New York City", new DateOnly(2025, 12, 27), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateOnly(2025, 12, 20) }
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "CategoryId", "MasterUserId", "Name" },
                values: new object[,]
                {
                    { 1, 1, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Toothbrush" },
                    { 2, 1, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Toothpaste" },
                    { 3, 1, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Shampoo" },
                    { 4, 1, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Deodorant" },
                    { 5, 1, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Razor" },
                    { 6, 1, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Face Wash" },
                    { 7, 1, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Floss" },
                    { 8, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "T-shirts" },
                    { 9, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Jeans" },
                    { 10, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Sweater" },
                    { 11, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Raincoat" },
                    { 12, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Socks" },
                    { 13, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Underwear" },
                    { 14, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Pajamas" },
                    { 15, 2, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Hat" },
                    { 16, 3, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Phone Charger" },
                    { 17, 3, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Power Bank" },
                    { 18, 3, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Headphones" },
                    { 19, 3, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Laptop" },
                    { 20, 3, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Kindle" },
                    { 21, 3, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "USB Cable" },
                    { 22, 4, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Hiking Boots" },
                    { 23, 4, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Tent" },
                    { 24, 4, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Sleeping Bag" },
                    { 25, 4, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Flashlight" },
                    { 26, 4, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Water Bottle" },
                    { 27, 4, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Backpack" },
                    { 28, 5, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Granola Bars" },
                    { 29, 5, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Trail Mix" },
                    { 30, 5, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Jerky" },
                    { 31, 5, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Fruit Snacks" },
                    { 32, 5, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Protein Bars" },
                    { 33, 6, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Passport" },
                    { 34, 6, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Boarding Pass" },
                    { 35, 6, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Travel Insurance" },
                    { 36, 6, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Itinerary" },
                    { 37, 6, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "ID Card" }
                });

            migrationBuilder.InsertData(
                table: "TripItems",
                columns: new[] { "Id", "IsPacked", "ItemId", "TripId" },
                values: new object[,]
                {
                    { 1, false, 8, 1 },
                    { 2, true, 9, 1 },
                    { 3, false, 10, 1 },
                    { 4, true, 11, 1 },
                    { 5, false, 12, 1 },
                    { 6, true, 13, 1 },
                    { 7, false, 14, 1 },
                    { 8, true, 15, 1 },
                    { 9, false, 16, 1 },
                    { 10, true, 17, 1 },
                    { 11, false, 18, 1 },
                    { 12, true, 19, 1 },
                    { 13, false, 20, 1 },
                    { 14, true, 21, 1 },
                    { 15, false, 22, 1 },
                    { 16, true, 23, 1 },
                    { 17, false, 24, 1 },
                    { 18, true, 25, 1 },
                    { 19, false, 26, 1 },
                    { 20, true, 27, 1 },
                    { 21, false, 28, 1 },
                    { 22, true, 29, 1 },
                    { 23, false, 30, 1 },
                    { 24, true, 31, 1 },
                    { 25, false, 32, 1 },
                    { 26, false, 1, 2 },
                    { 27, true, 2, 2 },
                    { 28, false, 3, 2 },
                    { 29, true, 4, 2 },
                    { 30, false, 5, 2 },
                    { 31, true, 6, 2 },
                    { 32, false, 7, 2 },
                    { 33, true, 8, 2 },
                    { 34, false, 9, 2 },
                    { 35, true, 10, 2 },
                    { 36, false, 11, 2 },
                    { 37, true, 12, 2 },
                    { 38, false, 13, 2 },
                    { 39, true, 14, 2 },
                    { 40, false, 15, 2 },
                    { 41, true, 16, 2 },
                    { 42, false, 17, 2 },
                    { 43, true, 18, 2 },
                    { 44, false, 19, 2 },
                    { 45, true, 20, 2 },
                    { 46, false, 21, 2 },
                    { 47, true, 22, 2 },
                    { 48, false, 23, 2 },
                    { 49, true, 24, 2 },
                    { 50, false, 25, 2 },
                    { 51, true, 26, 2 },
                    { 52, false, 27, 2 },
                    { 53, true, 28, 2 },
                    { 54, false, 29, 2 },
                    { 55, true, 30, 2 },
                    { 56, false, 31, 2 },
                    { 57, true, 32, 2 },
                    { 58, false, 8, 3 },
                    { 59, true, 9, 3 },
                    { 60, false, 10, 3 },
                    { 61, true, 11, 3 },
                    { 62, false, 12, 3 },
                    { 63, true, 13, 3 },
                    { 64, false, 14, 3 },
                    { 65, true, 15, 3 },
                    { 66, false, 16, 3 },
                    { 67, true, 17, 3 },
                    { 68, false, 18, 3 },
                    { 69, true, 19, 3 },
                    { 70, false, 20, 3 },
                    { 71, true, 21, 3 },
                    { 72, false, 22, 3 },
                    { 73, true, 23, 3 },
                    { 74, false, 24, 3 },
                    { 75, true, 25, 3 },
                    { 76, false, 26, 3 },
                    { 77, true, 27, 3 },
                    { 78, false, 1, 4 },
                    { 79, true, 2, 4 },
                    { 80, false, 3, 4 },
                    { 81, true, 4, 4 },
                    { 82, false, 5, 4 },
                    { 83, true, 6, 4 },
                    { 84, false, 7, 4 },
                    { 85, true, 8, 4 },
                    { 86, false, 9, 4 },
                    { 87, true, 10, 4 },
                    { 88, false, 11, 4 },
                    { 89, true, 12, 4 },
                    { 90, false, 13, 4 },
                    { 91, true, 14, 4 },
                    { 92, false, 15, 4 },
                    { 93, true, 16, 4 },
                    { 94, false, 17, 4 },
                    { 95, true, 18, 4 },
                    { 96, false, 19, 4 },
                    { 97, true, 20, 4 },
                    { 98, false, 21, 4 },
                    { 99, true, 22, 4 },
                    { 100, false, 23, 4 },
                    { 101, true, 24, 4 },
                    { 102, false, 25, 4 },
                    { 103, true, 26, 4 },
                    { 104, false, 27, 4 },
                    { 105, false, 8, 5 },
                    { 106, true, 9, 5 },
                    { 107, false, 10, 5 },
                    { 108, true, 11, 5 },
                    { 109, false, 12, 5 },
                    { 110, true, 13, 5 },
                    { 111, false, 14, 5 },
                    { 112, true, 15, 5 },
                    { 113, false, 16, 5 },
                    { 114, true, 17, 5 },
                    { 115, false, 18, 5 },
                    { 116, true, 19, 5 },
                    { 117, false, 20, 5 },
                    { 118, true, 21, 5 },
                    { 119, false, 22, 5 },
                    { 120, true, 23, 5 },
                    { 121, false, 24, 5 },
                    { 122, true, 25, 5 },
                    { 123, false, 26, 5 },
                    { 124, true, 27, 5 },
                    { 125, false, 28, 5 },
                    { 126, true, 29, 5 },
                    { 127, false, 30, 5 },
                    { 128, true, 31, 5 },
                    { 129, false, 32, 5 },
                    { 130, true, 33, 5 },
                    { 131, false, 34, 5 },
                    { 132, true, 35, 5 },
                    { 133, false, 36, 5 },
                    { 134, true, 37, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserLogins_MasterUserId",
                table: "AppUserLogins",
                column: "MasterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MasterUserId",
                table: "Categories",
                column: "MasterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId",
                table: "Items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_MasterUserId",
                table: "Items",
                column: "MasterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsers_IdentityUserId",
                table: "MasterUsers",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TripItems_ItemId",
                table: "TripItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TripItems_TripId",
                table: "TripItems",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_MasterUserId",
                table: "Trips",
                column: "MasterUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "TripItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "MasterUsers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
