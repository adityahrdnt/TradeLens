using System.ComponentModel.DataAnnotations;

namespace TradeLens.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(1)]
    [RegularExpression(@".*\S.*")]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    [RegularExpression(@".*\S.*")]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32)]
    [RegularExpression(@".*\S.*")]
    public string SecretKey { get; init; } = string.Empty;
}
