using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class renamingConversationRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversation_AspNetUsers_LowerUserId",
                table: "Conversation");

            migrationBuilder.RenameColumn(
                name: "LowerUserId",
                table: "Conversation",
                newName: "LowerUserId_init");

            migrationBuilder.RenameIndex(
                name: "IX_Conversation_LowerUserId",
                table: "Conversation",
                newName: "IX_Conversation_LowerUserId_init");

            migrationBuilder.RenameIndex(
                name: "IX_Conversation_HigherUserId_LowerUserId",
                table: "Conversation",
                newName: "IX_Conversation_HigherUserId_LowerUserId_init");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversation_AspNetUsers_LowerUserId_init",
                table: "Conversation",
                column: "LowerUserId_init",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversation_AspNetUsers_LowerUserId_init",
                table: "Conversation");

            migrationBuilder.RenameColumn(
                name: "LowerUserId_init",
                table: "Conversation",
                newName: "LowerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Conversation_LowerUserId_init",
                table: "Conversation",
                newName: "IX_Conversation_LowerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Conversation_HigherUserId_LowerUserId_init",
                table: "Conversation",
                newName: "IX_Conversation_HigherUserId_LowerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversation_AspNetUsers_LowerUserId",
                table: "Conversation",
                column: "LowerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
