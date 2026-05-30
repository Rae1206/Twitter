using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.SqlServer.Repositories;

/// <summary>
/// Repositorio de base de datos encargado de la persistencia, consulta y almacenamiento del historial conversacional de los usuarios con el chatbot de IA.
/// </summary>
public class ChatbotMessageRepository(TwitterDbContext context)
    : GenericRepository<ChatbotMessage, Guid>(context), IChatbotMessageRepository
{
    /// <summary>
    /// Obtiene de forma asíncrona el historial completo de mensajes entre el usuario y el chatbot, paginado y ordenado por fecha de creación (de más antiguo a más nuevo).
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad máxima de mensajes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>La lista de mensajes <see cref="ChatbotMessage"/> resultantes de la consulta.</returns>
    public async Task<List<ChatbotMessage>> GetHistoryAsync(
        Guid userId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0
            ? Shared.Constants.ChatbotConstants.DefaultHistoryLimit
            : Math.Min(limit, Shared.Constants.ChatbotConstants.MaxHistoryLimit);

        return await _context.ChatbotMessages
            .Where(message => message.UserId == userId)
            .OrderBy(message => message.CreatedAt)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Recupera de forma asíncrona la lista de mensajes más recientes de la conversación de un usuario para contextualizar la llamada con el proveedor de IA.
    /// Retorna los mensajes ordenados cronológicamente (antiguos a recientes).
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Límite de cantidad de mensajes contextuales.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Lista de mensajes ordenados cronológicamente.</returns>
    public async Task<List<ChatbotMessage>> GetRecentConversationAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = limit <= 0 ? Shared.Constants.ChatbotConstants.RecentContextMessageLimit : limit;

        var recentMessages = await _context.ChatbotMessages
            .Where(message => message.UserId == userId)
            .OrderByDescending(message => message.CreatedAt)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        return recentMessages
            .OrderBy(message => message.CreatedAt)
            .ToList();
    }
}
