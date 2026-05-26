using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Shared.Constants;
using Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class MessageService(
    IUnitOfWork unitOfWork,
    ILogger<MessageService> logger) : IMessageService
{
    public async Task<Message> SendMessage(Guid senderId, Guid receiverId, string content)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User {SenderId} sending message to {ReceiverId}", senderId, receiverId);
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

        return message;
    }

    public async Task<List<Message>> GetConversation(Guid userId1, Guid userId2, int limit = 0, int offset = 0)
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

        return await unitOfWork.Messages.GetConversation(userId1, userId2, limit, offset);
    }

    public async Task<List<Message>> GetUnreadMessages(Guid receiverId)
    {
        return await unitOfWork.Messages.GetUnreadMessages(receiverId);
    }

    public async Task<int> GetUnreadCount(Guid receiverId)
    {
        return await unitOfWork.Messages.GetUnreadCount(receiverId);
    }

    public async Task MarkAsRead(Guid messageId)
    {
        await unitOfWork.Messages.MarkAsRead(messageId);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task MarkConversationAsRead(Guid senderId, Guid receiverId)
    {
        await unitOfWork.Messages.MarkConversationAsRead(senderId, receiverId);
        await unitOfWork.SaveChangesAsync();
    }
}