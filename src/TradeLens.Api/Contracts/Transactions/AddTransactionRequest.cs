using TradeLens.Domain.Enums;

namespace TradeLens.Api.Contracts.Transactions;

public sealed record AddTransactionRequest(
    Guid PortfolioId,
    Guid BrokerAccountId,
    Guid InstrumentId,
    TransactionType Type,
    long Quantity,
    decimal Price,
    decimal Fee,
    DateOnly TransactionDate,
    long Sequence);