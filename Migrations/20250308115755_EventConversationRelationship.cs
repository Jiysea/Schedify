using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedify.Migrations
{
    /// <inheritdoc />
    public partial class EventConversationRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_EventId",
                table: "Conversations");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_EventId",
                table: "Conversations",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_EventId",
                table: "Conversations");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_EventId",
                table: "Conversations",
                column: "EventId");
        }
    }
}
