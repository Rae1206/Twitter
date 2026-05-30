using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Shared.Constants;
using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio para gestionar los seguimientos (follows) entre usuarios.
/// </summary>
public class FollowService(
    IUnitOfWork unitOfWork,
    ILogger<FollowService> logger) : IFollowService
{
    /// <summary>
    /// Establece una relación de seguimiento donde un usuario comienza a seguir a otro.
    /// Actualiza los contadores de seguidores y seguidos desnormalizados de ambos usuarios.
    /// </summary>
    /// <param name="followerId">Identificador único del usuario seguidor.</param>
    /// <param name="followingId">Identificador único del usuario al que se desea seguir.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task FollowUser(Guid followerId, Guid followingId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User {FollowerId} following {FollowingId}", followerId, followingId);
        }

        if (followerId == followingId)
        {
            throw new BadRequestException("No puedes seguirte a ti mismo");
        }

        var follower = await unitOfWork.Users.GetByIdAsync(followerId);
        if (follower is null)
        {
            throw new ResourceNotFoundException("user", followerId);
        }

        var following = await unitOfWork.Users.GetByIdAsync(followingId);
        if (following is null)
        {
            throw new ResourceNotFoundException("user", followingId);
        }

        var existing = await unitOfWork.Follows.GetFollow(followerId, followingId);
        if (existing is not null)
        {
            throw new ConflictException(ErrorConstants.ALREADY_FOLLOWING);
        }

        try
        {
            var follow = new Follow
            {
                FollowId = Guid.NewGuid(),
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTimeHelper.UtcNow()
            };
            unitOfWork.Create(follow);

            // Actualiza los contadores desnormalizados
            follower.FollowingCount++;
            following.FollowersCount++;
            unitOfWork.Update(follower);
            unitOfWork.Update(following);

            await unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Follows_FollowerId_FollowingId") == true || ex.InnerException?.Message.Contains("unique") == true)
        {
            throw new ConflictException(ErrorConstants.ALREADY_FOLLOWING);
        }
    }

    /// <summary>
    /// Elimina una relación de seguimiento existente (dejar de seguir a un usuario).
    /// Disminuye los contadores desnormalizados de seguidores y seguidos correspondientes.
    /// </summary>
    /// <param name="followerId">Identificador único del usuario seguidor.</param>
    /// <param name="followingId">Identificador único del usuario al que se deja de seguir.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task UnfollowUser(Guid followerId, Guid followingId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User {FollowerId} unfollowing {FollowingId}", followerId, followingId);
        }

        var follow = await unitOfWork.Follows.GetFollow(followerId, followingId);
        if (follow is null)
        {
            throw new BadRequestException(ErrorConstants.NOT_FOLLOWING);
        }

        var follower = await unitOfWork.Users.GetByIdAsync(followerId);
        var following = await unitOfWork.Users.GetByIdAsync(followingId);

        unitOfWork.Delete(follow);

        // Actualiza los contadores desnormalizados
        if (follower is not null)
        {
            follower.FollowingCount = Math.Max(0, follower.FollowingCount - 1);
            unitOfWork.Update(follower);
        }
        if (following is not null)
        {
            following.FollowersCount = Math.Max(0, following.FollowersCount - 1);
            unitOfWork.Update(following);
        }

        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene la lista de seguidores de un usuario determinado de forma paginada y los retorna mapeados como DTOs.
    /// </summary>
    /// <param name="userId">Identificador único del usuario del que se consultan los seguidores.</param>
    /// <param name="limit">Cantidad máxima de seguidores a recuperar.</param>
    /// <param name="offset">Cantidad de seguidores a omitir para la paginación.</param>
    /// <returns>La lista de seguidores del usuario en forma de DTOs.</returns>
    public async Task<List<UserDto>> GetFollowers(Guid userId, int limit = 0, int offset = 0)
    {
        var users = await unitOfWork.Follows.GetFollowers(userId, limit, offset);
        return users.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Obtiene la lista paginada de usuarios a los que sigue un usuario determinado y los retorna mapeados como DTOs.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad máxima de usuarios seguidos a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista de usuarios seguidos en forma de DTOs.</returns>
    public async Task<List<UserDto>> GetFollowing(Guid userId, int limit = 0, int offset = 0)
    {
        var users = await unitOfWork.Follows.GetFollowing(userId, limit, offset);
        return users.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Obtiene el número total de seguidores que posee un usuario determinado.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La cantidad total de seguidores.</returns>
    public async Task<int> GetFollowersCount(Guid userId)
    {
        return await unitOfWork.Follows.GetFollowersCount(userId);
    }

    /// <summary>
    /// Obtiene el número total de usuarios a los que sigue un usuario determinado.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La cantidad total de usuarios a los que sigue.</returns>
    public async Task<int> GetFollowingCount(Guid userId)
    {
        return await unitOfWork.Follows.GetFollowingCount(userId);
    }

    /// <summary>
    /// Verifica de forma asíncrona si existe una relación de seguimiento activa de un usuario hacia otro.
    /// </summary>
    /// <param name="followerId">Identificador único del usuario seguidor.</param>
    /// <param name="followingId">Identificador único del usuario seguido.</param>
    /// <returns>Verdadero si el usuario seguidor sigue al usuario seguido; de lo contrario, falso.</returns>
    public async Task<bool> IsFollowing(Guid followerId, Guid followingId)
    {
        return await unitOfWork.Follows.IsFollowing(followerId, followingId);
    }

    /// <summary>
    /// Mapea de forma interna una entidad de base de datos User a su representación estructurada de DTO.
    /// </summary>
    /// <param name="user">Entidad User a mapear.</param>
    /// <returns>El DTO de usuario correspondiente.</returns>
    private static UserDto MapToDto(User user) => new()
    {
        UserId = user.UserId,
        Nickname = user.Nickname,
        Email = user.Email,
        Biography = user.Biography,
        ProfilePhotoUrl = user.ProfilePhotoUrl,
        IsActive = user.IsActive,
        IsSuspended = user.IsSuspended,
        IsShadowBanned = user.IsShadowBanned,
        DeletedAt = user.DeletedAt,
        FollowersCount = user.FollowersCount,
        FollowingCount = user.FollowingCount,
        CreatedAt = user.CreatedAt
    };
}