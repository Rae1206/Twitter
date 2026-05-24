using System;
using System.Collections.Generic;
using Twitter.Domain.Database.SqlServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace Twitter.Domain.Database.SqlServer.Context;

public partial class TwitterDbContext : DbContext
{
    public TwitterDbContext()
    {
    }

    public TwitterDbContext(DbContextOptions<TwitterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<AdminAuditLog> AdminAuditLogs { get; set; }

    public virtual DbSet<UserSuspension> UserSuspensions { get; set; }

    public virtual DbSet<ContentReport> ContentReports { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<AdminDashboardStat> AdminDashboardStats { get; set; }

    public virtual DbSet<AdminSession> AdminSessions { get; set; }

    public virtual DbSet<PostMedia> PostMedias { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Posts__AA126018962A6902");

            entity.HasIndex(e => e.IsPublished, "IX_Posts_IsPublished");

            entity.HasIndex(e => e.UserId, "IX_Posts_UserId");

            entity.HasIndex(e => e.RepliedToPostId, "IX_Posts_RepliedToPostId");

            entity.HasIndex(e => e.RetweetOfPostId, "IX_Posts_RetweetOfPostId");

            entity.HasIndex(e => e.ExpiresAt, "IX_Posts_ExpiresAt")
                .HasFilter("[ExpiresAt] IS NOT NULL");

            entity.Property(e => e.PostId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsPublished).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.RepliedToPost).WithMany(p => p.Replies)
                .HasForeignKey(d => d.RepliedToPostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.RetweetOfPost).WithMany(p => p.Retweets)
                .HasForeignKey(d => d.RetweetOfPostId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8A586D7E8D93F157");

            entity.HasIndex(e => e.Name, "IX_Roles_Name").IsUnique();

            entity.Property(e => e.RoleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CDE3FE834");

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.Property(e => e.Biography).HasMaxLength(500);
            entity.Property(e => e.ProfilePhotoFileName).HasMaxLength(255);
            entity.Property(e => e.ProfilePhotoStoragePath).HasMaxLength(500);
            entity.Property(e => e.ProfilePhotoUrl).HasMaxLength(1000);
            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRoles__9B71D8E8D93F157");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "IX_UserRoles_UserId_RoleId").IsUnique();

            entity.Property(e => e.UserRoleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.EmailTemplateId).HasName("PK__EmailTemplates__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.Name, "IX_EmailTemplates_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissions__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.Name, "IX_Permissions_Name").IsUnique();

            entity.Property(e => e.PermissionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Module).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("PK__RolePermissions__6D6FDC4E8D93F157");

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "IX_RolePermissions_RoleId_PermissionId").IsUnique();

            entity.Property(e => e.RolePermissionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AdminAuditLogs__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.AdminUserId, "IX_AdminAuditLogs_AdminUserId");
            entity.HasIndex(e => e.Action, "IX_AdminAuditLogs_Action");
            entity.HasIndex(e => e.EntityType, "IX_AdminAuditLogs_EntityType");
            entity.HasIndex(e => e.CreatedAt, "IX_AdminAuditLogs_CreatedAt");

            entity.Property(e => e.AuditLogId).HasColumnName("AuditId").HasDefaultValueSql("(newid())");
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AdminUser).WithMany()
                .HasForeignKey(d => d.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable("AdminAuditLog");
        });

        modelBuilder.Entity<UserSuspension>(entity =>
        {
            entity.HasKey(e => e.SuspensionId).HasName("PK__UserSuspensions__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.UserId, "IX_UserSuspensions_UserId");
            entity.HasIndex(e => e.AdminUserId, "IX_UserSuspensions_AdminUserId");
            entity.HasIndex(e => e.IsActive, "IX_UserSuspensions_IsActive");

            entity.Property(e => e.SuspensionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SuspensionType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.AdminUser).WithMany()
                .HasForeignKey(d => d.AdminUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ContentReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__ContentReports__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.ReporterId, "IX_ContentReports_ReporterId");
            entity.HasIndex(e => e.TargetType, "IX_ContentReports_TargetType");
            entity.HasIndex(e => e.Status, "IX_ContentReports_Status");
            entity.HasIndex(e => e.AssignedTo, "IX_ContentReports_AssignedTo");

            entity.Property(e => e.ReportId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TargetType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Resolution).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Reporter).WithMany()
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__SystemConfigs__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.Key, "IX_SystemConfig_ConfigKey").IsUnique();

            entity.Property(e => e.ConfigId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Key).HasColumnName("ConfigKey").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasColumnName("ConfigValue").IsRequired();
            entity.Property(e => e.ValueType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.UpdatedByUserId).HasColumnName("UpdatedByUserId");

            entity.ToTable("SystemConfig");
        });

        modelBuilder.Entity<AdminDashboardStat>(entity =>
        {
            entity.HasKey(e => e.StatId).HasName("PK__AdminDashboardStats__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.StatKey, "IX_AdminDashboardStats_StatKey").IsUnique();

            entity.Property(e => e.StatId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.StatKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastCalculated).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<AdminSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__AdminSessions__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.AdminUserId, "IX_AdminSessions_AdminUserId");

            entity.Property(e => e.SessionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.AdminUser).WithMany()
                .HasForeignKey(d => d.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostMedia>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PK__PostMedias__6D6FDC4E8D93F157");

            entity.HasIndex(e => e.PostId, "IX_PostMedias_PostId");
            entity.HasIndex(e => e.UserId, "IX_PostMedias_UserId");
            entity.HasIndex(e => e.CreatedAt, "IX_PostMedias_CreatedAt");

            entity.Property(e => e.MediaId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Post).WithMany(p => p.PostMedias)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.ToTable("PostMedia");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK_Likes");

            entity.HasIndex(e => new { e.UserId, e.PostId }, "IX_Likes_UserId_PostId").IsUnique();
            entity.HasIndex(e => e.PostId, "IX_Likes_PostId");

            entity.Property(e => e.LikeId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable("Likes");
        });

        // Global query filters for soft delete + ephemeral expiry.
        // EF Core translates DateTime.UtcNow to SYSUTCDATETIME() in SQL Server,
        // so the expiry check is evaluated on the database server in real time (no client-side cache, no gap).
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
        modelBuilder.Entity<Post>().HasQueryFilter(p => p.DeletedAt == null && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));
        modelBuilder.Entity<PostMedia>().HasQueryFilter(m => !m.IsDeleted);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
