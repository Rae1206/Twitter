using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Servicio encargado de la gestión de la mensajería directa (DMs) entre los usuarios del sistema.
/// </summary>
public class MessageService(
    IUnitOfWork unitOfWork,
    ILogger<MessageService> logger) : IMessageService
{
    /// <summary>
    /// Envía de forma asíncrona un nuevo mensaje directo de un usuario emisor a un usuario receptor.
    /// Realiza validaciones de existencia de los usuarios y límites del mensaje.
    /// </summary>
    /// <param name="senderId">Identificador único del usuario que envía el mensaje.</param>
    /// <param name="receiverId">Identificador único del usuario que recibe el mensaje.</param>
    /// <param name="content">El contenido del mensaje directo.</param>
    /// <returns>La representación en formato DTO del mensaje enviado.</returns>
    public async Task<MessageDto> SendMessage(Guid senderId, Guid receiverId, string content)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario {SenderId} enviando mensaje a {ReceiverId}", senderId, receiverId);
        }

        var sender = await unitOfWork.Users.GetByIdAsync(senderId);
        if (sender is null)
        {
            throw new ResourceNotFoundException("user", senderId);
        }

        var receiver = await unitOfWork.Users.GetByIdAsync(receiverId);
        if (receiver is null)
        {
            throw new ResourceNotFoundException("user", receiverId);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new BadRequestException("El mensaje no puede estar vacío");
        }

        if (content.Length > 1000)
        {
            throw new BadRequestException("El mensaje no puede exceder los 1000 caracteres");
        }

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            IsRead = false,
            DeletedBySender = false,
            DeletedByReceiver = false,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(message);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(message, sender, receiver);
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista de mensajes intercambiados entre dos usuarios específicos en una conversación, ordenados y paginados.
    /// </summary>
    /// <param name="userId1">Identificador único del primer usuario.</param>
    /// <param name="userId2">Identificador único del segundo usuario.</param>
    /// <param name="limit">Cantidad máxima de mensajes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de DTOs con la conversación recuperada.</returns>
    public async Task<List<MessageDto>> GetConversation(Guid userId1, Guid userId2, int limit = 0, int offset = 0)
    {
        var user1 = await unitOfWork.Users.GetByIdAsync(userId1);
        if (user1 is null)
        {
            throw new ResourceNotFoundException("user", userId1);
        }

        var user2 = await unitOfWork.Users.GetByIdAsync(userId2);
        if (user2 is null)
        {
            throw new ResourceNotFoundException("user", userId2);
        }

        var messages = await unitOfWork.Messages.GetConversationAsync(userId1, userId2, limit, offset);
        return messages.Select(m => MapToDto(m, m.Sender, m.Receiver)).ToList();
    }

    /// <summary>
    /// Recupera de forma asíncrona el listado de las últimas conversaciones activas para un usuario (con el último mensaje de cada una), paginado.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad máxima de conversaciones a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista de DTOs representando las conversaciones del usuario.</returns>
    public async Task<List<MessageDto>> GetConversationsList(Guid userId, int limit = 0, int offset = 0)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("user", userId);
        }

        var messages = await unitOfWork.Messages.GetConversationsListAsync(userId, limit, offset);
        return messages.Select(m => MapToDto(m, m.Sender, m.Receiver)).ToList();
    }

    /// <summary>
    /// Obtiene de forma asíncrona todos los mensajes marcados como no leídos dirigidos al usuario receptor.
    /// </summary>
    /// <param name="receiverId">Identificador del usuario receptor.</param>
    /// <returns>Lista de DTOs de mensajes no leídos.</returns>
    public async Task<List<MessageDto>> GetUnreadMessages(Guid receiverId)
    {
        var messages = await unitOfWork.Messages.GetUnreadMessages(receiverId);
        return messages.Select(m => MapToDto(m, m.Sender, m.Receiver)).ToList();
    }

    /// <summary>
    /// Cuenta de forma asíncrona la cantidad total de mensajes no leídos que ha recibido el usuario receptor.
    /// </summary>
    /// <param name="receiverId">Identificador del usuario receptor.</param>
    /// <returns>La cantidad total de mensajes no leídos.</returns>
    public async Task<int> GetUnreadCount(Guid receiverId)
    {
        return await unitOfWork.Messages.CountUnreadAsync(receiverId);
    }

    /// <summary>
    /// Obtiene de forma asíncrona la cantidad de mensajes no leídos en una conversación específica con otro usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario que consulta.</param>
    /// <param name="otherUserId">Identificador del otro participante de la conversación.</param>
    /// <returns>La cantidad de mensajes no leídos en esa conversación.</returns>
    public async Task<int> GetUnreadCountInConversation(Guid userId, Guid otherUserId)
    {
        return await unitOfWork.Messages.CountUnreadInConversationAsync(userId, otherUserId);
    }

    /// <summary>
    /// Marca de forma asíncrona un mensaje individual específico como leído en la base de datos.
    /// </summary>
    /// <param name="messageId">Identificador del mensaje.</param>
    /// <param name="userId">Identificador del usuario que realiza la acción.</param>
    public async Task MarkAsRead(Guid messageId, Guid userId)
    {
        await unitOfWork.Messages.MarkAsReadAsync(messageId);
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Marca un mensaje como leído y devuelve el recibo de lectura para notificar al emisor vía SignalR.
    /// </summary>
    /// <param name="messageId">Identificador del mensaje.</param>
    /// <param name="userId">Identificador del usuario que lee el mensaje (receptor).</param>
    /// <returns>DTO con la información del recibo de lectura, o null si el mensaje no existe o ya estaba leído.</returns>
    public async Task<MessageReadDto?> MarkAsReadWithReceipt(Guid messageId, Guid userId)
    {
        var message = await unitOfWork.Messages.MarkAsReadAsync(messageId);
        if (message is null) return null;

        await unitOfWork.SaveChangesAsync();

        return new MessageReadDto
        {
            MessageId = message.MessageId,
            ReadBy = userId,
            SenderId = message.SenderId,
            ReadAt = message.ReadAt!.Value
        };
    }

    /// <summary>
    /// Marca todos los mensajes de una conversación como leídos y devuelve un recibo de lectura para notificar al emisor.
    /// </summary>
    /// <param name="userId">Identificador del usuario que lee (receptor).</param>
    /// <param name="otherUserId">Identificador del otro usuario (emisor).</param>
    /// <returns>DTO con la información del recibo de lectura, o null si no había mensajes sin leer.</returns>
    public async Task<MessageReadDto?> MarkConversationAsReadWithReceipt(Guid userId, Guid otherUserId)
    {
        await unitOfWork.Messages.MarkConversationAsReadAsync(userId, otherUserId);
        await unitOfWork.SaveChangesAsync();

        return new MessageReadDto
        {
            MessageId = null,
            ReadBy = userId,
            SenderId = otherUserId,
            ReadAt = DateTimeHelper.UtcNow()
        };
    }

    /// <summary>
    /// Marca de forma asíncrona todos los mensajes en una conversación específica como leídos.
    /// </summary>
    /// <param name="userId">Identificador del usuario destinatario.</param>
    /// <param name="otherUserId">Identificador del usuario emisor en la conversación.</param>
    public async Task MarkConversationAsRead(Guid userId, Guid otherUserId)
    {
        await unitOfWork.Messages.MarkConversationAsReadAsync(userId, otherUserId);
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Realiza de forma asíncrona una eliminación lógica del mensaje para el usuario actual (emisor o receptor), ocultándoselo en su historial.
    /// </summary>
    /// <param name="messageId">Identificador del mensaje a eliminar.</param>
    /// <param name="userId">Identificador del usuario que solicita la eliminación.</param>
    public async Task DeleteMessage(Guid messageId, Guid userId)
    {
        await unitOfWork.Messages.DeleteForUserAsync(messageId, userId);
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Método de asistencia privado para mapear la entidad de mensaje de base de datos junto con la información del emisor y receptor a su correspondiente DTO.
    /// </summary>
    /// <param name="message">Entidad de mensaje.</param>
    /// <param name="sender">Entidad del usuario emisor.</param>
    /// <param name="receiver">Entidad del usuario receptor.</param>
    /// <returns>La representación DTO <see cref="MessageDto"/>.</returns>
    private static MessageDto MapToDto(Message message, User sender, User receiver)
    {
        return new MessageDto
        {
            MessageId = message.MessageId,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            SenderUsername = sender.Nickname,
            ReceiverUsername = receiver.Nickname,
            SenderAvatar = sender.ProfilePhotoUrl,
            ReceiverAvatar = receiver.ProfilePhotoUrl,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }
}
