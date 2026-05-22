using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Database.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPostMediaSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminAuditLogs_Users_AdminUserId",
                table: "AdminAuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "IsEditable",
                table: "SystemConfigs");

            migrationBuilder.RenameTable(
                name: "SystemConfigs",
                newName: "SystemConfig");

            migrationBuilder.RenameTable(
                name: "AdminAuditLogs",
                newName: "AdminAuditLog");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SystemConfig",
                newName: "ConfigValue");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "SystemConfig",
                newName: "ConfigKey");

            migrationBuilder.RenameIndex(
                name: "IX_SystemConfigs_Key",
                table: "SystemConfig",
                newName: "IX_SystemConfig_ConfigKey");

            migrationBuilder.RenameColumn(
                name: "AuditLogId",
                table: "AdminAuditLog",
                newName: "AuditId");

            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "SystemConfig",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "SystemConfig",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueType",
                table: "SystemConfig",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PostMedia",
                columns: table => new
                {
                    MediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PostMedias__6D6FDC4E8D93F157", x => x.MediaId);
                    table.ForeignKey(
                        name: "FK_PostMedia_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostMedias_CreatedAt",
                table: "PostMedia",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostMedias_PostId",
                table: "PostMedia",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostMedias_UserId",
                table: "PostMedia",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminAuditLog_Users_AdminUserId",
                table: "AdminAuditLog",
                column: "AdminUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.InsertData(
                table: "SystemConfig",
                columns: new[] { "ConfigId", "ConfigKey", "ConfigValue", "ValueType", "Module", "Description" },
                values: new object[] { Guid.NewGuid(), "Post.MaxMediaCount", "4", "int", "Post", "Maximum number of media files per post" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminAuditLog_Users_AdminUserId",
                table: "AdminAuditLog");

            migrationBuilder.DropTable(
                name: "PostMedia");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "ValueType",
                table: "SystemConfig");

            migrationBuilder.RenameTable(
                name: "SystemConfig",
                newName: "SystemConfigs");

            migrationBuilder.RenameTable(
                name: "AdminAuditLog",
                newName: "AdminAuditLogs");

            migrationBuilder.RenameColumn(
                name: "ConfigValue",
                table: "SystemConfigs",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "ConfigKey",
                table: "SystemConfigs",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_SystemConfig_ConfigKey",
                table: "SystemConfigs",
                newName: "IX_SystemConfigs_Key");

            migrationBuilder.RenameColumn(
                name: "AuditId",
                table: "AdminAuditLogs",
                newName: "AuditLogId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SystemConfigs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<bool>(
                name: "IsEditable",
                table: "SystemConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminAuditLogs_Users_AdminUserId",
                table: "AdminAuditLogs",
                column: "AdminUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
