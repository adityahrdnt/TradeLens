using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TradeLens.Api.Authentication;

public sealed class DevelopmentAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "development-user")
        };

        var identity = new ClaimsIdentity(
            claims,
            Scheme.Name);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}