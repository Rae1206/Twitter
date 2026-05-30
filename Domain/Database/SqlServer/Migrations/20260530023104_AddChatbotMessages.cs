using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Database.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddChatbotMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatbotMessages",
                columns: table => new
                {
                    ChatbotMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotMessages", x => x.ChatbotMessageId);
                    table.ForeignKey(
                        name: "FK_ChatbotMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotMessages_UserId",
                table: "ChatbotMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotMessages_UserId_CreatedAt",
                table: "ChatbotMessages",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatbotMessages");
        }
    }
}
