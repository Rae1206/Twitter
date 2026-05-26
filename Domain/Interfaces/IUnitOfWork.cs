using Domain.Interfaces.Repositories;
using Twitter.Domain.Interfaces.Repositories;

namespace Twitter.Domain.Interfaces;

/// <summary>
/// Unit of Work interface.
/// Defines write operations and repository access.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    IAuthRepository Auth { get; }
    IRoleRepository Roles { get; }
    IEmailTemplateRepository EmailTemplates { get; }
    IPermissionRepository Permissions { get; }
    IRolePermissionRepository RolePermissions { get; }
    IAdminAuditLogRepository AdminAuditLogs { get; }
    IUserSuspensionRepository UserSuspensions { get; }
    IContentReportRepository ContentReports { get; }
    ISystemConfigRepository SystemConfigs { get; }
    IAdminDashboardStatRepository AdminDashboardStats { get; }
    IAdminSessionRepository AdminSessions { get; }
    IPostMediaRepository PostMedias { get; }
    ILikeRepository Likes { get; }

    IFollowRepository Follows { get; }
    IMessageRepository Messages { get; }


    void Create<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;

    Task SaveChangesAsync();
}
