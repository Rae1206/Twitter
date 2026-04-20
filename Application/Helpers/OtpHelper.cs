using System.Security.Cryptography;

namespace Application.Helpers;

/// <summary>
/// Helper para generar y validar OTPs.
/// </summary>
public static class OtpHelper
{
    private const int OTP_LENGTH = 6;
    private const int OTP_EXPIRATION_MINUTES = 15;

    /// <summary>
    /// Genera un OTP de 6 dígitos.
    /// </summary>
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        var value = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return value.ToString("D6");
    }

    /// <summary>
    /// Genera la clave para almacenar el OTP en cache.
    /// </summary>
    public static string GetCacheKey(string email)
    {
        return $"otp:{email}";
    }

    /// <summary>
    /// Obtiene el tiempo de expiración del OTP.
    /// </summary>
    public static TimeSpan Expiration => TimeSpan.FromMinutes(OTP_EXPIRATION_MINUTES);
}