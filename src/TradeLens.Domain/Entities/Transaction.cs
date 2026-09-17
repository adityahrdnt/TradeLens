using TradeLens.Domain.Enums;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }

    public Guid PortfolioId { get; private set; }

    public Guid BrokerAccountId { get; private set; }

    public Guid InstrumentId { get; private set; }

    public TransactionType Type { get; private set; }

    public long Quantity { get; private set; }

    public decimal Price { get; private set; }

    public decimal Fee { get; private set; }

    public DateOnly TransactionDate { get; private set; }

    public long Sequence { get; private set; }

    public TransactionStatus Status { get; private set; }

    public Guid? SupersedesTransactionId { get; private set; }

    public string? CorrectionReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? SupersededAt { get; private set; }

    public Guid? SupersededBy { get; private set; }

    public string? VoidReason { get; private set; }

    private Transaction()
    {
    }

    public Transaction(
        Guid id,
        Guid portfolioId,
        Guid brokerAccountId,
        Guid instrumentId,
        TransactionType type,
        long quantity,
        decimal price,
        decimal fee,
        DateOnly transactionDate,
        long sequence,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("Transaction id is required.");

        if (portfolioId == Guid.Empty)
            throw new DomainException("Portfolio id is required.");

        if (brokerAccountId == Guid.Empty)
            throw new DomainException("Broker account id is required.");

        if (instrumentId == Guid.Empty)
            throw new DomainException("Instrument id is required.");

        if (quantity <= 0)
            throw new DomainException("Transaction quantity must be greater than zero.");

        if (price < 0)
            throw new DomainException("Transaction price cannot be negative.");

        if (fee < 0)
            throw new DomainException("Transaction fee cannot be negative.");

        if (sequence <= 0)
            throw new DomainException("Transaction sequence must be greater than zero.");

        if (createdBy == Guid.Empty)
            throw new DomainException("CreatedBy is required.");

        Id = id;
        PortfolioId = portfolioId;
        BrokerAccountId = brokerAccountId;
        InstrumentId = instrumentId;

        Type = type;
        Quantity = quantity;
        Price = price;
        Fee = fee;

        TransactionDate = transactionDate;
        Sequence = sequence;

        Status = TransactionStatus.Active;

        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    public void Supersede(
        DateTimeOffset supersededAt,
        Guid supersededBy,
        string reason)
    {
        if (Status != TransactionStatus.Active)
            throw new DomainException(
                "Only an active transaction can be superseded.");

        if (supersededBy == Guid.Empty)
            throw new DomainException(
                "SupersededBy is required.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(
                "Correction reason is required.");

        Status = TransactionStatus.Superseded;
        SupersededAt = supersededAt;
        SupersededBy = supersededBy;
        CorrectionReason = reason.Trim();
    }

    public static Transaction CreateCorrection(
        Guid id,
        Transaction original,
        long quantity,
        decimal price,
        decimal fee,
        DateOnly transactionDate,
        long sequence,
        Guid createdBy,
        DateTimeOffset createdAt,
        string reason)
    {
        if (original.Status != TransactionStatus.Active)
            throw new DomainException(
                "Only an active transaction can be corrected.");

        if (id == Guid.Empty)
            throw new DomainException("Transaction id is required.");

        if (createdBy == Guid.Empty)
            throw new DomainException("CreatedBy is required.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Correction reason is required.");

        return new Transaction(
            id,
            original.PortfolioId,
            original.BrokerAccountId,
            original.InstrumentId,
            original.Type,
            quantity,
            price,
            fee,
            transactionDate,
            sequence,
            createdBy,
            createdAt)
        {
            SupersedesTransactionId = original.Id,
            CorrectionReason = reason.Trim()
        };
    }
}