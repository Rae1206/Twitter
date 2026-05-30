using System.Text.Json;
using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Helpers;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio encargado de registrar y listar bitácoras de auditoría de las acciones realizadas por los administradores.
/// </summary>
public class AuditService(
    IUnitOfWork unitOfWork,
    ILogger<AuditService> logger) : IAuditService
{
    /// <summary>
    /// Registra de forma asíncrona un cambio administrativo en la bitácora de auditoría.
    /// </summary>
    /// <param name="adminId">Identificador único del administrador que realiza la acción.</param>
    /// <param name="action">Nombre o tipo de la acción administrativa efectuada.</param>
    /// <param name="entityType">Tipo de entidad afectada (ej. "Post", "User").</param>
    /// <param name="entityId">Identificador único en formato string de la entidad afectada.</param>
    /// <param name="oldValue">El valor previo del recurso antes del cambio.</param>
    /// <param name="newValue">El nuevo valor asignado al recurso.</param>
    /// <param name="reason">Motivo o justificación opcional del cambio.</param>
    /// <returns>Una tarea asíncrona que representa el proceso de registro.</returns>
    public async Task LogChangeAsync(Guid adminId, string action, string entityType, string? entityId, object? oldValue, object? newValue, string? reason = null)
    {
        try
        {
            var log = new AdminAuditLog
            {
                AuditLogId = Guid.NewGuid(),
                AdminUserId = adminId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = oldValue is not null ? JsonSerializer.Serialize(oldValue) : null,
                NewValue = newValue is not null ? JsonSerializer.Serialize(newValue) : null,
                Reason = reason,
                CreatedAt = DateTimeHelper.UtcNow()
            };

            unitOfWork.Create(log);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al escribir en audit log | Action: {Action}, Admin: {AdminId}", action, adminId);
        }
    }

    /// <summary>
    /// Obtiene de forma paginada y filtrada los registros de auditoría almacenados en el sistema.
    /// </summary>
    /// <param name="limit">Cantidad máxima de registros de auditoría a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="adminUserId">Filtro opcional por identificador de administrador.</param>
    /// <param name="action">Filtro opcional por tipo de acción realizada.</param>
    /// <param name="entityType">Filtro opcional por tipo de entidad afectada.</param>
    /// <param name="dateFrom">Filtro opcional para obtener registros desde esta fecha.</param>
    /// <param name="dateTo">Filtro opcional para obtener registros hasta esta fecha.</param>
    /// <returns>Un sobre genérico de respuesta que envuelve la lista de registros de auditoría encontrados.</returns>
    public async Task<GenericResponse<List<AdminAuditLog>>> GetAuditLogsAsync(int limit, int offset, Guid? adminUserId = null, string? action = null, string? entityType = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var logs = await unitOfWork.AdminAuditLogs.GetPagedAsync(limit, offset, adminUserId, action, entityType, dateFrom, dateTo);
        return new GenericResponse<List<AdminAuditLog>> { Data = logs };
    }
}
