using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de mensajes del chatbot de IA, heredando operaciones genéricas para la entidad <see cref="ChatbotMessage"/>.
/// </summary>
public interface IChatbotMessageRepository : IGenericRepository<ChatbotMessage, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona el historial completo de la conversación de un usuario con el chatbot de forma paginada y ordenada cronológicamente.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad máxima de mensajes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="cancellationToken">Token opcional de cancelación de la operación asíncrona.</param>
    /// <returns>Una lista conteniendo las entidades <see cref="ChatbotMessage"/> del historial.</returns>
    Task<List<ChatbotMessage>> GetHistoryAsync(Guid userId, int limit, int offset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera de forma asíncrona los mensajes más recientes de la conversación de un usuario para proveer contexto inmediato a la IA.
    /// Retorna los mensajes ordenados en orden cronológico ascendente.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad de mensajes recientes a recuperar.</param>
    /// <param name="cancellationToken">Token opcional de cancelación de la operación asíncrona.</param>
    /// <returns>Una lista conteniendo los últimos mensajes de la conversación.</returns>
    Task<List<ChatbotMessage>> GetRecentConversationAsync(Guid userId, int limit, CancellationToken cancellationToken = default);
}
