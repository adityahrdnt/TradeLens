using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? PasswordHash { get; private set; }

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    public int TokenVersion { get; private set; }

    public DateTimeOffset? EmailVerifiedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private User()
    {
    }

    private User(
        Guid id,
        string email,
        string displayName,
        string? passwordHash,
        UserRole role,
        DateTimeOffset now)
    {
        if (id == Guid.Empty)
            throw new DomainException("User ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        if (email.Trim().Length > 320)
            throw new DomainException("Email cannot exceed 320 characters.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required.");

        if (displayName.Trim().Length > 150)
            throw new DomainException("Display name cannot exceed 150 characters.");

        if (!Enum.IsDefined(role))
            throw new DomainException("Invalid user role.");

        if (role == UserRole.User && string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required for registered users.");

        Id = id;
        Email = email.Trim();
        NormalizedEmail = Email.ToUpperInvariant();
        DisplayName = displayName.Trim();
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.PendingActivation;
        TokenVersion = 1;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public static User CreateRegistration(
        Guid id,
        string email,
        string displayName,
        string passwordHash,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User(
            id,
            email,
            displayName,
            passwordHash,
            UserRole.User,
            now);
    }

    public static User CreateAdminInvitation(
        Guid id,
        string email,
        string displayName,
        DateTimeOffset now)
    {
        return new User(
            id,
            email,
            displayName,
            null,
            UserRole.Admin,
            now);
    }

    public static User CreateSuperAdmin(
        Guid id,
        string email,
        string displayName,
        string passwordHash,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        var user = new User(
            id,
            email,
            displayName,
            passwordHash,
            UserRole.SuperAdmin,
            now);

        // SuperAdmin is created through a controlled bootstrap process.
        user.Status = UserStatus.Active;
        user.EmailVerifiedAtUtc = now;

        return user;
    }

    public void SetPassword(string passwordHash, DateTimeOffset now)
    {
        if (Status != UserStatus.PendingActivation)
            throw new DomainException("Only pending users can set their password.");

        if (PasswordHash is not null)
            throw new DomainException("Password has already been set.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = passwordHash;
        UpdatedAtUtc = now;
    }

    public void Activate(DateTimeOffset now)
    {
        if (Status != UserStatus.PendingActivation)
            throw new DomainException("Only pending users can be activated.");

        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new DomainException("Password must be set before activation.");

        Status = UserStatus.Active;
        EmailVerifiedAtUtc = now;
        UpdatedAtUtc = now;

        IncrementTokenVersion();
    }

    public void Disable(DateTimeOffset now)
    {
        if (Status == UserStatus.Disabled)
            throw new DomainException("User is already disabled.");

        Status = UserStatus.Disabled;
        UpdatedAtUtc = now;

        IncrementTokenVersion();
    }

    public void ChangeRole(UserRole newRole, DateTimeOffset now)
    {
        if (Status != UserStatus.Active)
            throw new DomainException("Only active users can change roles.");

        if (!Enum.IsDefined(newRole))
            throw new DomainException("Invalid user role.");

        if (Role == newRole)
            throw new DomainException("User already has the requested role.");

        Role = newRole;
        UpdatedAtUtc = now;

        IncrementTokenVersion();
    }

    private void IncrementTokenVersion()
    {
        TokenVersion = checked(TokenVersion + 1);
    }
}
