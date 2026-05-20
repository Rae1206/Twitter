USE X_alex;
GO

-- =====================================================
-- Twitter Clone - MÓDULO DE ADMINISTRACIÓN
-- Extiende el schema existente con:
--   - Permisos granulares (Permissions + RolePermissions)
--   - Audit Log (toda acción de admin queda registrada)
--   - Reportes de usuarios/posts
--   - Suspensiones y bans con historial
--   - Configuración global del sistema
--   - Dashboard stats (caché de métricas)
--   - Moderación de contenido
-- =====================================================

-- =====================================================
-- 1. Permissions - Permisos granulares
-- =====================================================
-- Permite definir acciones específicas por módulo,
-- más flexible que solo tener el rol "Admin"
IF OBJECT_ID('dbo.Permissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Permissions (
        PermissionId   UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name           NVARCHAR(100)  NOT NULL UNIQUE,
        -- Ejemplos: 'users.view' | 'users.edit' | 'users.delete'
        --           'users.ban' | 'posts.view' | 'posts.delete'
        --           'posts.restore' | 'reports.manage' | 'config.edit'
        --           'audit.view' | 'roles.manage'
        Module         NVARCHAR(50)   NOT NULL,   -- 'users' | 'posts' | 'reports' | 'config' | 'audit'
        Description    NVARCHAR(255)  NULL,
        CreatedAt      DATETIME2      DEFAULT GETUTCDATE()
    );
    PRINT 'OK tabla dbo.Permissions';
END
GO

-- Relación N:N entre Roles y Permissions
IF OBJECT_ID('dbo.RolePermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions (
        RolePermissionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        RoleId           UNIQUEIDENTIFIER NOT NULL,
        PermissionId     UNIQUEIDENTIFIER NOT NULL,
        GrantedAt        DATETIME2        DEFAULT GETUTCDATE(),
        GrantedByUserId  UNIQUEIDENTIFIER NULL,    -- Quién asignó el permiso
        CONSTRAINT UQ_RolePermission UNIQUE (RoleId, PermissionId),
        FOREIGN KEY (RoleId)          REFERENCES dbo.Roles(RoleId)       ON DELETE CASCADE,
        FOREIGN KEY (PermissionId)    REFERENCES dbo.Permissions(PermissionId) ON DELETE CASCADE,
        FOREIGN KEY (GrantedByUserId) REFERENCES dbo.Users(UserId)
    );
    PRINT 'OK tabla dbo.RolePermissions';
END
GO

-- =====================================================
-- 2. AdminAuditLog - Trazabilidad completa
-- =====================================================
-- Cada acción que ejecuta un admin queda registrada:
-- quién hizo qué, sobre qué entidad, cuándo y desde dónde
IF OBJECT_ID('dbo.AdminAuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminAuditLog (
        AuditId        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        AdminUserId    UNIQUEIDENTIFIER NOT NULL,    -- Admin que ejecutó la acción
        Action         NVARCHAR(100)    NOT NULL,
        -- 'USER_BANNED' | 'USER_UNBANNED' | 'USER_EDITED'
        -- 'USER_DELETED' | 'USER_VERIFIED' | 'USER_ROLE_CHANGED'
        -- 'POST_DELETED' | 'POST_RESTORED' | 'REPORT_RESOLVED'
        -- 'CONFIG_UPDATED' | 'PERMISSION_GRANTED' | 'PERMISSION_REVOKED'
        EntityType     NVARCHAR(50)     NOT NULL,    -- 'User' | 'Post' | 'Report' | 'Config'
        EntityId       NVARCHAR(100)    NULL,        -- GUID de la entidad afectada (como string para flexibilidad)
        OldValue       NVARCHAR(MAX)    NULL,        -- JSON del estado anterior
        NewValue       NVARCHAR(MAX)    NULL,        -- JSON del estado nuevo
        Reason         NVARCHAR(500)    NULL,        -- Justificación del admin (obligatorio para bans)
        IpAddress      NVARCHAR(50)     NULL,
        UserAgent      NVARCHAR(500)    NULL,
        CreatedAt      DATETIME2        DEFAULT GETUTCDATE(),
        FOREIGN KEY (AdminUserId) REFERENCES dbo.Users(UserId)
    );
    PRINT 'OK tabla dbo.AdminAuditLog';
END
GO

-- =====================================================
-- 3. UserSuspensions - Bans con historial
-- =====================================================
-- Historial completo de suspensiones por usuario.
-- IsActive = 1 significa que está actualmente suspendido.
IF OBJECT_ID('dbo.UserSuspensions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserSuspensions (
        SuspensionId   UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId         UNIQUEIDENTIFIER NOT NULL,    -- Usuario suspendido
        AdminUserId    UNIQUEIDENTIFIER NOT NULL,    -- Admin que suspendió
        Reason         NVARCHAR(500)    NOT NULL,
        SuspensionType NVARCHAR(20)     NOT NULL DEFAULT 'temporary',
        -- 'temporary' = tiene fecha de fin
        -- 'permanent' = ban permanente
        -- 'shadow'    = shadowban (usuario no sabe que está baneado)
        StartsAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        EndsAt         DATETIME2        NULL,        -- NULL = permanente
        IsActive       BIT              NOT NULL DEFAULT 1,
        LiftedAt       DATETIME2        NULL,        -- Cuándo fue levantado
        LiftedByUserId UNIQUEIDENTIFIER NULL,        -- Admin que levantó el ban
        LiftReason     NVARCHAR(500)    NULL,
        CreatedAt      DATETIME2        DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId)         REFERENCES dbo.Users(UserId),
        FOREIGN KEY (AdminUserId)    REFERENCES dbo.Users(UserId),
        FOREIGN KEY (LiftedByUserId) REFERENCES dbo.Users(UserId)
    );
    PRINT 'OK tabla dbo.UserSuspensions';
END
GO

-- Columna en Users para saber rápido si está suspendido (evita JOIN en cada request)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'IsSuspended')
    ALTER TABLE dbo.Users ADD IsSuspended     BIT           NOT NULL DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'SuspendedUntil')
    ALTER TABLE dbo.Users ADD SuspendedUntil  DATETIME2     NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'IsShadowBanned')
    ALTER TABLE dbo.Users ADD IsShadowBanned  BIT           NOT NULL DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'DeletedAt')
    ALTER TABLE dbo.Users ADD DeletedAt       DATETIME2     NULL;    -- Soft delete de usuario
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'DeletedByAdminId')
    ALTER TABLE dbo.Users ADD DeletedByAdminId UNIQUEIDENTIFIER NULL;
GO

-- =====================================================
-- 4. ContentReports - Reportes de usuarios/posts
-- =====================================================
IF OBJECT_ID('dbo.ContentReports', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContentReports (
        ReportId       UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ReporterUserId UNIQUEIDENTIFIER NOT NULL,    -- Quien reportó
        EntityType     NVARCHAR(20)     NOT NULL,    -- 'Post' | 'User' | 'Message'
        EntityId       UNIQUEIDENTIFIER NOT NULL,    -- ID de lo reportado
        Category       NVARCHAR(50)     NOT NULL,
        -- 'spam' | 'hate_speech' | 'harassment' | 'misinformation'
        -- 'nudity' | 'violence' | 'copyright' | 'other'
        Description    NVARCHAR(500)    NULL,
        Status         NVARCHAR(20)     NOT NULL DEFAULT 'pending',
        -- 'pending' | 'under_review' | 'resolved' | 'dismissed'
        Priority       TINYINT          NOT NULL DEFAULT 2,         -- 1=Alta 2=Media 3=Baja
        AssignedToAdminId UNIQUEIDENTIFIER NULL,     -- Admin asignado para revisar
        Resolution     NVARCHAR(500)    NULL,        -- Nota del admin al resolver
        ResolvedAt     DATETIME2        NULL,
        ResolvedByAdminId UNIQUEIDENTIFIER NULL,
        CreatedAt      DATETIME2        DEFAULT GETUTCDATE(),
        FOREIGN KEY (ReporterUserId)      REFERENCES dbo.Users(UserId),
        FOREIGN KEY (AssignedToAdminId)   REFERENCES dbo.Users(UserId),
        FOREIGN KEY (ResolvedByAdminId)   REFERENCES dbo.Users(UserId)
    );
    PRINT 'OK tabla dbo.ContentReports';
END
GO

-- Columna en Posts para saber si tiene reportes activos (para cola de moderación)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'ReportCount')
    ALTER TABLE dbo.Posts ADD ReportCount   INT NOT NULL DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsFlagged')
    ALTER TABLE dbo.Posts ADD IsFlagged     BIT NOT NULL DEFAULT 0;  -- Admin lo marcó para revisión
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'DeletedByAdminId')
    ALTER TABLE dbo.Posts ADD DeletedByAdminId UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'DeletedReason')
    ALTER TABLE dbo.Posts ADD DeletedReason NVARCHAR(255) NULL;
GO

-- =====================================================
-- 5. SystemConfig - Configuración global editable
-- =====================================================
-- El admin puede modificar parámetros del sistema
-- sin redesplegar la app (feature flags, límites, etc.)
IF OBJECT_ID('dbo.SystemConfig', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemConfig (
        ConfigId       UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ConfigKey      NVARCHAR(100)    NOT NULL UNIQUE,
        ConfigValue    NVARCHAR(MAX)    NOT NULL,
        ValueType      NVARCHAR(20)     NOT NULL DEFAULT 'string',
        -- 'string' | 'int' | 'bool' | 'json'
        Description    NVARCHAR(500)    NULL,
        Module         NVARCHAR(50)     NULL,        -- 'posts' | 'users' | 'media' | 'ai'
        UpdatedAt      DATETIME2        DEFAULT GETUTCDATE(),
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.Users(UserId)
    );
    PRINT 'OK tabla dbo.SystemConfig';
END
GO

-- =====================================================
-- 6. AdminDashboardStats - Caché de métricas
-- =====================================================
-- Evita queries pesadas en tiempo real al dashboard.
-- Un job las recalcula cada hora/día.
IF OBJECT_ID('dbo.AdminDashboardStats', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminDashboardStats (
        StatId         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StatKey        NVARCHAR(100)    NOT NULL,    -- 'total_users' | 'active_users_today' | etc.
        StatValue      NVARCHAR(MAX)    NOT NULL,    -- Valor como string (puede ser JSON)
        Period         NVARCHAR(20)     NOT NULL DEFAULT 'realtime',
        -- 'realtime' | 'hourly' | 'daily' | 'weekly' | 'monthly'
        PeriodDate     DATE             NULL,        -- Para stats diarias/semanales
        CalculatedAt   DATETIME2        DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_AdminStat UNIQUE (StatKey, Period, PeriodDate)
    );
    PRINT 'OK tabla dbo.AdminDashboardStats';
END
GO

-- =====================================================
-- 7. AdminSessions - Sesiones del panel admin
-- =====================================================
-- Seguridad extra: rastrear cada inicio de sesión al panel
IF OBJECT_ID('dbo.AdminSessions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminSessions (
        SessionId      UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        AdminUserId    UNIQUEIDENTIFIER NOT NULL,
        IpAddress      NVARCHAR(50)     NULL,
        UserAgent      NVARCHAR(500)    NULL,
        LoginAt        DATETIME2        DEFAULT GETUTCDATE(),
        LogoutAt       DATETIME2        NULL,
        IsActive       BIT              NOT NULL DEFAULT 1,
        FOREIGN KEY (AdminUserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
    );
    PRINT 'OK tabla dbo.AdminSessions';
END
GO

-- =====================================================
-- NUEVOS ROLES PARA ADMINISTRACIÓN
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Moderator')
BEGIN
    INSERT INTO dbo.Roles (Name, Description, IsActive) VALUES 
        ('Moderator', 'Moderador de contenido - puede revisar reportes y eliminar posts', 1),
        ('SuperAdmin', 'Super administrador - acceso total incluyendo configuración del sistema', 1);
    PRINT 'OK roles Moderator y SuperAdmin';
END
GO

-- =====================================================
-- PERMISOS GRANULARES
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = 'users.view')
BEGIN
    INSERT INTO dbo.Permissions (Name, Module, Description) VALUES
    -- Módulo Users
    ('users.view',          'users',   'Ver listado y detalle de usuarios'),
    ('users.edit',          'users',   'Editar perfil y datos de usuarios'),
    ('users.delete',        'users',   'Eliminar usuarios (soft delete)'),
    ('users.ban',           'users',   'Suspender o banear usuarios'),
    ('users.verify',        'users',   'Otorgar o quitar verificación'),
    ('users.roles',         'users',   'Cambiar roles de usuarios'),
    -- Módulo Posts
    ('posts.view',          'posts',   'Ver todos los posts incluyendo eliminados'),
    ('posts.delete',        'posts',   'Eliminar posts de cualquier usuario'),
    ('posts.restore',       'posts',   'Restaurar posts eliminados'),
    ('posts.flag',          'posts',   'Marcar posts para revisión'),
    -- Módulo Reports
    ('reports.view',        'reports', 'Ver reportes de contenido'),
    ('reports.manage',      'reports', 'Gestionar y resolver reportes'),
    ('reports.assign',      'reports', 'Asignar reportes a moderadores'),
    -- Módulo Config
    ('config.view',         'config',  'Ver configuración del sistema'),
    ('config.edit',         'config',  'Editar configuración del sistema'),
    -- Módulo Audit
    ('audit.view',          'audit',   'Ver logs de auditoría'),
    -- Módulo Dashboard
    ('dashboard.view',      'dashboard','Ver métricas del dashboard admin');
    PRINT 'OK inserts de Permissions';
END
GO

-- Asignar todos los permisos al rol Admin
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name = 'Admin'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId
  );

-- Asignar todos los permisos al rol SuperAdmin
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name = 'SuperAdmin'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId
  );

-- Asignar solo permisos de moderación al Moderator
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Roles r
JOIN dbo.Permissions p ON p.Name IN (
    'users.view', 'users.ban',
    'posts.view', 'posts.delete', 'posts.flag',
    'reports.view', 'reports.manage',
    'dashboard.view'
)
WHERE r.Name = 'Moderator'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId
  );
GO
PRINT 'OK asignación de permisos a roles';

-- =====================================================
-- CONFIGURACIÓN INICIAL DEL SISTEMA
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM dbo.SystemConfig WHERE ConfigKey = 'max_post_length')
BEGIN
    INSERT INTO dbo.SystemConfig (ConfigKey, ConfigValue, ValueType, Description, Module) VALUES
    ('max_post_length',           '280',     'int',    'Máximo de caracteres por post',              'posts'),
    ('max_media_per_post',        '4',       'int',    'Máximo de imágenes por post',                'posts'),
    ('max_audio_duration_sec',    '60',      'int',    'Duración máxima de audio en segundos',       'media'),
    ('max_video_duration_sec',    '140',     'int',    'Duración máxima de video en segundos',       'media'),
    ('max_video_size_mb',         '512',     'int',    'Tamaño máximo de video en MB',               'media'),
    ('ephemeral_max_duration_h',  '168',     'int',    'Máximo de horas para posts efímeros (7d)',   'posts'),
    ('ai_summary_enabled',        'true',    'bool',   'Habilitar resúmenes de hilos con IA',        'ai'),
    ('ai_model',                  'gpt-4o-mini', 'string', 'Modelo de IA para resúmenes',            'ai'),
    ('registration_enabled',      'true',    'bool',   'Permitir nuevos registros',                  'users'),
    ('maintenance_mode',          'false',   'bool',   'Modo mantenimiento (bloquea acceso público)','system'),
    ('reports_auto_flag_threshold','5',      'int',    'Nro de reportes para auto-marcar un post',   'reports'),
    ('default_user_role',         'User',    'string', 'Rol asignado por defecto al registrarse',    'users');
    PRINT 'OK inserts de SystemConfig';
END
GO

-- =====================================================
-- EMAIL TEMPLATES PARA ADMINISTRACIÓN
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM dbo.EmailTemplates WHERE Name = 'AccountSuspended')
BEGIN
    INSERT INTO dbo.EmailTemplates (Name, Subject, Body) VALUES
    ('AccountSuspended',
     'Tu cuenta ha sido suspendida',
     '<h1>Cuenta suspendida</h1><p>Hola {fullName}, tu cuenta ha sido suspendida. Razón: {reason}. Hasta: {endsAt}.</p>'),
    ('AccountBannedPermanent',
     'Tu cuenta ha sido bloqueada permanentemente',
     '<h1>Cuenta bloqueada</h1><p>Hola {fullName}, tu cuenta ha sido bloqueada permanentemente. Razón: {reason}.</p>'),
    ('AccountRestored',
     'Tu cuenta ha sido reactivada',
     '<h1>Cuenta reactivada</h1><p>Hola {fullName}, la suspensión de tu cuenta ha sido levantada.</p>'),
    ('PostRemoved',
     'Tu publicación ha sido eliminada',
     '<h1>Publicación eliminada</h1><p>Hola {fullName}, una de tus publicaciones fue eliminada. Razón: {reason}.</p>');
    PRINT 'OK email templates de admin';
END
GO

-- =====================================================
-- ÍNDICES ADMIN
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_AdminUserId')
    CREATE INDEX IX_AuditLog_AdminUserId ON dbo.AdminAuditLog(AdminUserId, CreatedAt DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_EntityType')
    CREATE INDEX IX_AuditLog_EntityType ON dbo.AdminAuditLog(EntityType, EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reports_Status')
    CREATE INDEX IX_Reports_Status ON dbo.ContentReports(Status, Priority, CreatedAt);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reports_EntityType')
    CREATE INDEX IX_Reports_EntityType ON dbo.ContentReports(EntityType, EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Suspensions_UserId')
    CREATE INDEX IX_Suspensions_UserId ON dbo.UserSuspensions(UserId, IsActive);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Suspensions_Active')
    CREATE INDEX IX_Suspensions_Active ON dbo.UserSuspensions(EndsAt) WHERE IsActive = 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RolePermissions_RoleId')
    CREATE INDEX IX_RolePermissions_RoleId ON dbo.RolePermissions(RoleId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AdminSessions_AdminUserId')
    CREATE INDEX IX_AdminSessions_AdminUserId ON dbo.AdminSessions(AdminUserId, LoginAt DESC);
GO

PRINT '=================================================';
PRINT ' Módulo Admin completado exitosamente';
PRINT ' Tablas nuevas:';
PRINT '   Permissions, RolePermissions';
PRINT '   AdminAuditLog';
PRINT '   UserSuspensions';
PRINT '   ContentReports';
PRINT '   SystemConfig';
PRINT '   AdminDashboardStats';
PRINT '   AdminSessions';
PRINT ' Roles nuevos: Moderator, SuperAdmin';
PRINT ' Columnas en Users: IsSuspended, SuspendedUntil,';
PRINT '   IsShadowBanned, DeletedAt, DeletedByAdminId';
PRINT ' Columnas en Posts: ReportCount, IsFlagged,';
PRINT '   DeletedByAdminId, DeletedReason';
PRINT '=================================================';
GO
