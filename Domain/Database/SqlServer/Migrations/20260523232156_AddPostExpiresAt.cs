using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Database.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPostExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ExpiresAt",
                table: "Posts",
                column: "ExpiresAt",
                filter: "[ExpiresAt] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_ExpiresAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Posts");
        }
    }
}
