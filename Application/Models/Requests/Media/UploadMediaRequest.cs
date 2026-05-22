namespace Application.Models.Requests.Media;

public class UploadMediaRequest
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Length { get; set; }
}
