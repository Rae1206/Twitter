using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de mensajes.
/// Hereda de IGenericRepository para operaciones CRUD genéricas.
/// </summary>
public interface IMessageRepository : IGenericRepository<Message, Guid>
{
    // Obtener conversación entre dos usuarios (paginado)
    Task<List<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int limit, int offset);

    // Obtener lista de conversaciones (últimos mensajes con cada usuario)
    Task<List<Message>> GetConversationsListAsync(Guid userId, int limit, int offset);

    // Obtener mensajes no leídos de un usuario
    Task<List<Message>> GetUnreadMessages(Guid receiverId);

    // Contar mensajes no leídos de un usuario
    Task<int> CountUnreadAsync(Guid userId);

    // Contar mensajes no leídos en una conversación específica
    Task<int> CountUnreadInConversationAsync(Guid userId, Guid otherUserId);

    // Marcar mensaje como leído
    Task MarkAsReadAsync(Guid messageId);

    // Marcar todos los mensajes de una conversación como leídos
    Task MarkConversationAsReadAsync(Guid userId, Guid otherUserId);

    // Eliminar mensaje (soft delete)
    Task DeleteForUserAsync(Guid messageId, Guid userId);

    // Actualizar mensaje
    void Update(Message message);
}
