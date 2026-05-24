namespace Shared.Constants;

public static class MediaConstants
{
    public const int MaxImageSizeMb = 5;
    public const int MaxGifSizeMb = 5;
    public const int MaxAudioSizeMb = 10;
    public const int MaxVideoSizeMb = 50;

    public const int MaxMediaPerPost = 4;
    public const int MaxAudioPerPost = 1;

    public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    public static readonly string[] AllowedGifExtensions = { ".gif" };
    public static readonly string[] AllowedAudioExtensions = { ".mp3", ".wav", ".ogg", ".webm", ".m4a" };
    public static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".webm", ".m4v" };

    public static readonly string[] AllowedImageMimeTypes = { "image/jpeg", "image/png", "image/webp" };
    public static readonly string[] AllowedGifMimeTypes = { "image/gif" };
    public static readonly string[] AllowedAudioMimeTypes = { "audio/mpeg", "audio/wav", "audio/ogg", "audio/x-wav", "audio/webm", "audio/mp4" };
    public static readonly string[] AllowedVideoMimeTypes = { "video/mp4", "video/quicktime", "video/webm", "video/x-m4v" };
}
