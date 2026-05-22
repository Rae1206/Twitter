using Twitter.Domain.Database.SqlServer.Entities.Enums;

namespace Application.Models.DTOs;

public class MediaUploadDto
{
    public Guid MediaId { get; set; }
    public string Url { get; set; } = null!;
    public MediaType MediaType { get; set; }
}
