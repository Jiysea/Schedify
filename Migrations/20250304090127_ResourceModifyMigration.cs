using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedify.Migrations
{
    /// <inheritdoc />
    public partial class ResourceModifyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceCaterings");

            migrationBuilder.DropTable(
                name: "ResourceEquipments");

            migrationBuilder.DropTable(
                name: "ResourceFurnitures");

            migrationBuilder.DropTable(
                name: "ResourcePersonnels");

            migrationBuilder.DropTable(
                name: "ResourceVenues");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                            name: "Size",
                            table: "Resources",
                            type: "nvarchar(max)",
                            nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Resources",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Resources",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
            name: "CityMunicipality",
            table: "Resources",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
            
            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Resources",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);
                
            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuItems",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceItems",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shift",
                table: "Resources",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
                
            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "Resources",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "CityMunicipality",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Dimensions",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "MenuItems",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "PriceItems",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Shift",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "Resources");

            migrationBuilder.CreateTable(
                name: "ResourceCaterings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceItems = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCaterings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceCaterings_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceEquipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specifications = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceEquipments_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceFurnitures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceFurnitures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceFurnitures_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourcePersonnels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcePersonnels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourcePersonnels_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceVenues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CityMunicipality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceVenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceVenues_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCaterings_ResourceId",
                table: "ResourceCaterings",
                column: "ResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceEquipments_ResourceId",
                table: "ResourceEquipments",
                column: "ResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceFurnitures_ResourceId",
                table: "ResourceFurnitures",
                column: "ResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePersonnels_ResourceId",
                table: "ResourcePersonnels",
                column: "ResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVenues_ResourceId",
                table: "ResourceVenues",
                column: "ResourceId",
                unique: true);
        }
    }
}
