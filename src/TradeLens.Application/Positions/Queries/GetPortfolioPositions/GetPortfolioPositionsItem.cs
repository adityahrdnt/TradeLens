namespace TradeLens.Application.Positions.Queries.GetPortfolioPositions;

public sealed record GetPortfolioPositionsItem(
    Guid PositionId,
    Guid InstrumentId,
    long Quantity,
    decimal CostBasis,
    decimal AveragePrice);