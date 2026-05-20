using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Database.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserIdToUserSuspensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdminUserId",
                table: "UserSuspensions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserSuspensions_AdminUserId",
                table: "UserSuspensions",
                column: "AdminUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSuspensions_Users_AdminUserId",
                table: "UserSuspensions",
                column: "AdminUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSuspensions_Users_AdminUserId",
                table: "UserSuspensions");

            migrationBuilder.DropIndex(
                name: "IX_UserSuspensions_AdminUserId",
                table: "UserSuspensions");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "UserSuspensions");
        }
    }
}
