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

public class MessageService(
    IUnitOfWork unitOfWork,
    ILogger<MessageService> logger) : IMessageService
{
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

    public async Task<List<MessageDto>> GetUnreadMessages(Guid receiverId)
    {
        var messages = await unitOfWork.Messages.GetUnreadMessages(receiverId);
        return messages.Select(m => MapToDto(m, m.Sender, m.Receiver)).ToList();
    }

    public async Task<int> GetUnreadCount(Guid receiverId)
    {
        return await unitOfWork.Messages.CountUnreadAsync(receiverId);
    }

    public async Task<int> GetUnreadCountInConversation(Guid userId, Guid otherUserId)
    {
        return await unitOfWork.Messages.CountUnreadInConversationAsync(userId, otherUserId);
    }

    public async Task MarkAsRead(Guid messageId, Guid userId)
    {
        await unitOfWork.Messages.MarkAsReadAsync(messageId);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task MarkConversationAsRead(Guid userId, Guid otherUserId)
    {
        await unitOfWork.Messages.MarkConversationAsReadAsync(userId, otherUserId);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteMessage(Guid messageId, Guid userId)
    {
        await unitOfWork.Messages.DeleteForUserAsync(messageId, userId);
        await unitOfWork.SaveChangesAsync();
    }

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
