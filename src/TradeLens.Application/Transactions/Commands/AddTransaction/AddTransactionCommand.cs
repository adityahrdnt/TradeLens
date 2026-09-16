using TradeLens.Domain.Enums;

namespace TradeLens.Application.Transactions.Commands.AddTransaction;

public sealed record AddTransactionCommand(
    Guid PortfolioId,
    Guid BrokerAccountId,
    Guid InstrumentId,
    TransactionType Type,
    long Quantity,
    decimal Price,
    decimal Fee,
    DateOnly TransactionDate,
    long Sequence,
    Guid CreatedBy);