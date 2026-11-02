using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class UserActivationToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public ActivationTokenPurpose Purpose { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? UsedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    private UserActivationToken()
    {
    }

    public UserActivationToken(
        Guid id,
        Guid userId,
        string tokenHash,
        ActivationTokenPurpose purpose,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset now)
    {
        if (id == Guid.Empty)
            throw new DomainException("Activation token ID cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("User ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        if (tokenHash.Length != 64)
            throw new DomainException("Token hash must be a SHA-256 hexadecimal hash.");

        if (!tokenHash.All(Uri.IsHexDigit))
            throw new DomainException("Token hash must contain hexadecimal characters only.");

        if (!Enum.IsDefined(purpose))
            throw new DomainException("Invalid activation token purpose.");

        if (expiresAtUtc <= now)
            throw new DomainException("Activation token expiration must be in the future.");

        Id = id;
        UserId = userId;
        TokenHash = tokenHash.ToUpperInvariant();
        Purpose = purpose;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = now;
    }

    public bool IsValid(DateTimeOffset now, ActivationTokenPurpose expectedPurpose)
    {
        return UsedAtUtc is null
            && RevokedAtUtc is null
            && ExpiresAtUtc > now
            && Purpose == expectedPurpose;
    }

    public void MarkAsUsed(
        DateTimeOffset now,
        ActivationTokenPurpose expectedPurpose)
    {
        if (UsedAtUtc is not null)
            throw new DomainException("Activation token has already been used.");

        if (RevokedAtUtc is not null)
            throw new DomainException("Activation token has been revoked.");

        if (ExpiresAtUtc <= now)
            throw new DomainException("Activation token has expired.");

        if (Purpose != expectedPurpose)
            throw new DomainException("Activation token purpose does not match.");

        UsedAtUtc = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        if (UsedAtUtc is not null)
            throw new DomainException("Used activation token cannot be revoked.");

        if (RevokedAtUtc is not null)
            throw new DomainException("Activation token has already been revoked.");

        RevokedAtUtc = now;
    }
}
