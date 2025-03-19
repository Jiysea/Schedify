using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedify.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEventResourcesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventResources");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Resources",
                newName: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_EventId",
                table: "Resources",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Events_EventId",
                table: "Resources",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Events_EventId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_EventId",
                table: "Resources");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "Resources",
                newName: "UserId");

            migrationBuilder.CreateTable(
                name: "EventResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventResources_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventResources_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventResources_EventId",
                table: "EventResources",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventResources_ResourceId",
                table: "EventResources",
                column: "ResourceId");
        }
    }
}
