using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedify.Migrations
{
    /// <inheritdoc />
    public partial class RemovingPriceItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceItems",
                table: "Resources");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PriceItems",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
