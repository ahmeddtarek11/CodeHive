using System.ComponentModel.DataAnnotations;

namespace CodeHive.Infrastructure.Identity;

public sealed class JwtSettings
{
    // Section name in appsettings.json; keeps the binding string in one place.
    public const string SectionName = "Jwt";

    // HMAC-SHA256 requires a key of at least 256 bits (32 bytes / chars).
    [Required, MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int AccessTokenMinutes { get; set; } = 60;

    [Range(1, int.MaxValue)]
    public int RefreshTokenDays { get; set; } = 7;
}
