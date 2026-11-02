using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class RoleChangeAudit
{
    public Guid Id { get; private set; }

    public Guid ActorUserId { get; private set; }

    public Guid TargetUserId { get; private set; }

    public UserRole PreviousRole { get; private set; }

    public UserRole NewRole { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset ChangedAtUtc { get; private set; }

    private RoleChangeAudit()
    {
    }

    public RoleChangeAudit(
        Guid id,
        Guid actorUserId,
        Guid targetUserId,
        UserRole previousRole,
        UserRole newRole,
        string? reason,
        DateTimeOffset changedAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException("Role change audit ID cannot be empty.");

        if (actorUserId == Guid.Empty)
            throw new DomainException("Actor user ID cannot be empty.");

        if (targetUserId == Guid.Empty)
            throw new DomainException("Target user ID cannot be empty.");

        if (!Enum.IsDefined(previousRole))
            throw new DomainException("Invalid previous user role.");

        if (!Enum.IsDefined(newRole))
            throw new DomainException("Invalid new user role.");

        if (previousRole == newRole)
            throw new DomainException("Previous and new roles cannot be the same.");

        if (reason?.Length > 500)
            throw new DomainException("Reason cannot exceed 500 characters.");

        Id = id;
        ActorUserId = actorUserId;
        TargetUserId = targetUserId;
        PreviousRole = previousRole;
        NewRole = newRole;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedAtUtc = changedAtUtc;
    }
}
