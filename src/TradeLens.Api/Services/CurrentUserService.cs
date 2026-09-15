using System.Security.Claims;
using TradeLens.Application.Interfaces;

namespace TradeLens.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdValue =
                _httpContextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");
            }

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException(
                    "Authenticated user id is invalid.");
            }

            return userId;
        }
    }
}