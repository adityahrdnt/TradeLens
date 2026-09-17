using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class IdempotencyRecord
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string RequestHash { get; private set; } = string.Empty;

    public Guid TransactionId { get; private set; }

    public Guid PositionId { get; private set; }

    public long PositionQuantity { get; private set; }

    public decimal PositionCostBasis { get; private set; }

    public decimal PositionAveragePrice { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private IdempotencyRecord()
    {
    }

    public IdempotencyRecord(
        Guid id,
        Guid userId,
        string idempotencyKey,
        string requestHash,
        Guid transactionId,
        Guid positionId,
        long positionQuantity,
        decimal positionCostBasis,
        decimal positionAveragePrice,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException(
                "Idempotency record id is required.");

        if (userId == Guid.Empty)
            throw new DomainException(
                "Idempotency user id is required.");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new DomainException(
                "Idempotency key is required.");

        if (string.IsNullOrWhiteSpace(requestHash))
            throw new DomainException(
                "Request hash is required.");

        if (transactionId == Guid.Empty)
            throw new DomainException(
                "Transaction id is required.");

        if (positionId == Guid.Empty)
            throw new DomainException(
                "Position id is required.");

        if (positionQuantity < 0)
            throw new DomainException(
                "Position quantity cannot be negative.");

        if (positionCostBasis < 0)
            throw new DomainException(
                "Position cost basis cannot be negative.");

        if (positionAveragePrice < 0)
            throw new DomainException(
                "Position average price cannot be negative.");

        Id = id;
        UserId = userId;
        IdempotencyKey = idempotencyKey.Trim();
        RequestHash = requestHash;
        TransactionId = transactionId;
        PositionId = positionId;
        PositionQuantity = positionQuantity;
        PositionCostBasis = positionCostBasis;
        PositionAveragePrice = positionAveragePrice;
        CreatedAt = createdAt;
    }
}