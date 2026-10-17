using System.ComponentModel.DataAnnotations;
using TradeLens.Api.Authentication;

namespace TradeLens.Integration.Tests.Authentication;

public sealed class JwtOptionsValidationTests
{
    [Fact]
    public void ValidOptions_ShouldPassValidation()
    {
        var options = new JwtOptions
        {
            Issuer = "TradeLens",
            Audience = "TradeLens.Api",
            SecretKey = "TradeLens-Integration-Test-Secret-Key-At-Least-32"
        };

        var results = Validate(options);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyOrWhitespaceIssuer_ShouldFailValidation(string issuer)
    {
        var options = new JwtOptions
        {
            Issuer = issuer,
            Audience = "TradeLens.Api",
            SecretKey = "TradeLens-Integration-Test-Secret-Key-At-Least-32"
        };

        var results = Validate(options);

        Assert.NotEmpty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyOrWhitespaceAudience_ShouldFailValidation(string audience)
    {
        var options = new JwtOptions
        {
            Issuer = "TradeLens",
            Audience = audience,
            SecretKey = "TradeLens-Integration-Test-Secret-Key-At-Least-32"
        };

        var results = Validate(options);

        Assert.NotEmpty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("                                ")]
    [InlineData("short")]
    public void InvalidSecretKey_ShouldFailValidation(string secretKey)
    {
        var options = new JwtOptions
        {
            Issuer = "TradeLens",
            Audience = "TradeLens.Api",
            SecretKey = secretKey
        };

        var results = Validate(options);

        Assert.NotEmpty(results);
    }

    private static List<ValidationResult> Validate(JwtOptions options)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            options,
            new ValidationContext(options),
            results,
            validateAllProperties: true);

        return results;
    }
}
