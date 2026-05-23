using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence;

/// <summary>
/// Unit of Work con operaciones de ESCRITURA.
/// Usa DbContext internamente.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TwitterDbContext _context;

    public IUserRepository Users { get; }
    public IPostRepository Posts { get; }
    public IAuthRepository Auth { get; }
    public IRoleRepository Roles { get; }
    public IEmailTemplateRepository EmailTemplates { get; }
    public IPermissionRepository Permissions { get; }
    public IRolePermissionRepository RolePermissions { get; }
    public IAdminAuditLogRepository AdminAuditLogs { get; }
    public IUserSuspensionRepository UserSuspensions { get; }
    public IContentReportRepository ContentReports { get; }
    public ISystemConfigRepository SystemConfigs { get; }
    public IAdminDashboardStatRepository AdminDashboardStats { get; }
    public IAdminSessionRepository AdminSessions { get; }
    public IPostMediaRepository PostMedias { get; }
    public ILikeRepository Likes { get; }

    public UnitOfWork(
        TwitterDbContext context,
        IUserRepository userRepository,
        IPostRepository postRepository,
        IAuthRepository authRepository,
        IRoleRepository roleRepository,
        IEmailTemplateRepository emailTemplateRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IAdminAuditLogRepository adminAuditLogRepository,
        IUserSuspensionRepository userSuspensionRepository,
        IContentReportRepository contentReportRepository,
        ISystemConfigRepository systemConfigRepository,
        IAdminDashboardStatRepository adminDashboardStatRepository,
        IAdminSessionRepository adminSessionRepository,
        IPostMediaRepository postMediaRepository,
        ILikeRepository likeRepository)
    {
        _context = context;
        Users = userRepository;
        Posts = postRepository;
        Auth = authRepository;
        Roles = roleRepository;
        EmailTemplates = emailTemplateRepository;
        Permissions = permissionRepository;
        RolePermissions = rolePermissionRepository;
        AdminAuditLogs = adminAuditLogRepository;
        UserSuspensions = userSuspensionRepository;
        ContentReports = contentReportRepository;
        SystemConfigs = systemConfigRepository;
        AdminDashboardStats = adminDashboardStatRepository;
        AdminSessions = adminSessionRepository;
        PostMedias = postMediaRepository;
        Likes = likeRepository;
    }

    public void Create<T>(T entity) where T : class
    {
        _context.Add(entity);
    }

    public void Update<T>(T entity) where T : class
    {
        _context.Update(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
        _context.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
