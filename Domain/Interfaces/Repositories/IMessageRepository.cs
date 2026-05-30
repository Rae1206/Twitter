using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de mensajes directos (DMs), heredando de <see cref="IGenericRepository{Message, Guid}"/>.
/// </summary>
public interface IMessageRepository : IGenericRepository<Message, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona la lista paginada de mensajes intercambiados en la conversación de dos usuarios específicos, ordenados de más recientes a más antiguos.
    /// </summary>
    /// <param name="user1Id">Identificador único del primer usuario.</param>
    /// <param name="user2Id">Identificador único del segundo usuario.</param>
    /// <param name="limit">Cantidad máxima de mensajes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista conteniendo los mensajes <see cref="Message"/> intercambiados.</returns>
    Task<List<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int limit, int offset);

    /// <summary>
    /// Recupera de forma asíncrona la lista paginada de las conversaciones activas de un usuario, obteniendo únicamente el último mensaje de cada chat.
    /// </summary>
    /// <param name="userId">Identificador único del usuario consultado.</param>
    /// <param name="limit">Cantidad máxima de conversaciones a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de los últimos mensajes de cada conversación.</returns>
    Task<List<Message>> GetConversationsListAsync(Guid userId, int limit, int offset);

    /// <summary>
    /// Obtiene de forma asíncrona todos los mensajes marcados como no leídos dirigidos al usuario receptor especificado.
    /// </summary>
    /// <param name="receiverId">Identificador del usuario receptor.</param>
    /// <returns>Una lista conteniendo los mensajes no leídos.</returns>
    Task<List<Message>> GetUnreadMessages(Guid receiverId);

    /// <summary>
    /// Cuenta de forma asíncrona el total de mensajes no leídos que ha recibido el usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>La cantidad total de mensajes no leídos.</returns>
    Task<int> CountUnreadAsync(Guid userId);

    /// <summary>
    /// Cuenta de forma asíncrona la cantidad de mensajes no leídos que el usuario ha recibido específicamente de otro usuario determinado.
    /// </summary>
    /// <param name="userId">Identificador del usuario destinatario.</param>
    /// <param name="otherUserId">Identificador del usuario emisor en la conversación.</param>
    /// <returns>La cantidad de mensajes no leídos en esa conversación.</returns>
    Task<int> CountUnreadInConversationAsync(Guid userId, Guid otherUserId);

    /// <summary>
    /// Marca de forma asíncrona un mensaje individual específico como leído en la base de datos.
    /// </summary>
    /// <param name="messageId">Identificador del mensaje.</param>
    Task MarkAsReadAsync(Guid messageId);

    /// <summary>
    /// Marca de forma asíncrona todos los mensajes pendientes de lectura en una conversación con otro usuario como leídos.
    /// </summary>
    /// <param name="userId">Identificador del usuario destinatario.</param>
    /// <param name="otherUserId">Identificador del usuario emisor.</param>
    Task MarkConversationAsReadAsync(Guid userId, Guid otherUserId);

    /// <summary>
    /// Realiza de forma asíncrona la eliminación lógica de un mensaje para un participante (emisor o receptor) estableciendo las banderas correspondientes.
    /// </summary>
    /// <param name="messageId">Identificador único del mensaje.</param>
    /// <param name="userId">Identificador del usuario que solicita la eliminación.</param>
    Task DeleteForUserAsync(Guid messageId, Guid userId);

    /// <summary>
    /// Registra modificaciones de datos aplicadas a un mensaje directo.
    /// </summary>
    /// <param name="message">La entidad conteniendo los cambios.</param>
    void Update(Message message);
}
