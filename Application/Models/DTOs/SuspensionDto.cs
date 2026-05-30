using System;

namespace Application.Models.DTOs;

public class SuspensionDto
{
    public Guid SuspensionId { get; set; }
    public Guid UserId { get; set; }
    public Guid AdminUserId { get; set; }
    public string SuspensionType { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
    public Guid? LiftedByUserId { get; set; }
    public DateTime? LiftedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}