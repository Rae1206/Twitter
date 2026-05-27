using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Database.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssignedToAdminFromReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentReports_Users_AssignedToAdminId",
                table: "ContentReports");

            migrationBuilder.DropIndex(
                name: "IX_ContentReports_AssignedToAdminId",
                table: "ContentReports");

            migrationBuilder.DropColumn(
                name: "AssignedToAdminId",
                table: "ContentReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToAdminId",
                table: "ContentReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_AssignedToAdminId",
                table: "ContentReports",
                column: "AssignedToAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentReports_Users_AssignedToAdminId",
                table: "ContentReports",
                column: "AssignedToAdminId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
