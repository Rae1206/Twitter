using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Database.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AlignContentReportsWithSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==========================================
            // 1. ContentReports: drop indexes that depend on columns we'll modify
            // ==========================================
            migrationBuilder.DropForeignKey(
                name: "FK_ContentReports_Users_ReporterId",
                table: "ContentReports");

            // Drop indexes before renaming/altering the columns they reference
            migrationBuilder.DropIndex(
                name: "IX_ContentReports_Status",
                table: "ContentReports");

            migrationBuilder.DropIndex(
                name: "IX_ContentReports_TargetType",
                table: "ContentReports");

            // Drop old simple Follows indexes (will recreate with compound)
            migrationBuilder.DropIndex(
                name: "IX_Follows_FollowerId",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_Follows_FollowingId",
                table: "Follows");

            // ==========================================
            // 2. ContentReports: renombrar columnas existentes
            // ==========================================
            migrationBuilder.RenameColumn(
                name: "ReporterId",
                table: "ContentReports",
                newName: "ReporterUserId");

            migrationBuilder.RenameColumn(
                name: "TargetType",
                table: "ContentReports",
                newName: "EntityType");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "ContentReports",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "ContentReports",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "AssignedTo",
                table: "ContentReports",
                newName: "AssignedToAdminId");

            // ==========================================
            // 2. ContentReports: cambiar tipos y constraints
            // ==========================================

            // EntityType: nvarchar(50) → nvarchar(20) NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "ContentReports",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // EntityId: nvarchar(100) → uniqueidentifier NOT NULL
            // CAUTION: existing string data will be lost; column must be empty or this will fail.
            // The column is empty in production (new feature), so this is safe.
            migrationBuilder.AlterColumn<Guid>(
                name: "EntityId",
                table: "ContentReports",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            // Description: was Reason NOT NULL, now nullable
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ContentReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            // Status: nvarchar(50) → nvarchar(20) DEFAULT 'pending'
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ContentReports",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // ==========================================
            // 3. ContentReports: agregar columnas nuevas
            // ==========================================
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ContentReports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "other");

            migrationBuilder.AddColumn<byte>(
                name: "Priority",
                table: "ContentReports",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)2);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedByAdminId",
                table: "ContentReports",
                type: "uniqueidentifier",
                nullable: true);

            // ==========================================
            // 4. Create new indexes
            // ==========================================
            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ReporterUserId",
                table: "ContentReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_AssignedToAdminId",
                table: "ContentReports",
                column: "AssignedToAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status",
                table: "ContentReports",
                columns: new[] { "Status", "Priority", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_EntityType",
                table: "ContentReports",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId",
                table: "Follows",
                columns: new[] { "FollowerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowingId",
                table: "Follows",
                columns: new[] { "FollowingId", "CreatedAt" });

            // ==========================================
            // 5. Foreign keys for ContentReports
            // ==========================================
            migrationBuilder.AddForeignKey(
                name: "FK_ContentReports_Users_ReporterUserId",
                table: "ContentReports",
                column: "ReporterUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentReports_Users_AssignedToAdminId",
                table: "ContentReports",
                column: "AssignedToAdminId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentReports_Users_ResolvedByAdminId",
                table: "ContentReports",
                column: "ResolvedByAdminId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert is not supported for data-type changes (EntityId string → Guid).
            // If you need to revert, restore from backup.
            throw new NotSupportedException("Rolling back this migration is not supported. Restore from database backup if needed.");
        }
    }
}