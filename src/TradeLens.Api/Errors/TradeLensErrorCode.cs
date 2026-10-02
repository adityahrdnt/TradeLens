namespace TradeLens.Api.Errors;

public static class TradeLensErrorCode
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InvalidTransaction = "INVALID_TRANSACTION";
    public const string TransactionNotFound = "TRANSACTION_NOT_FOUND";
    public const string CorporateActionNotFound = "CORPORATE_ACTION_NOT_FOUND";
    public const string TransactionConflict = "TRANSACTION_CONFLICT";
    public const string MarketPriceNotAvailable = "MARKET_PRICE_NOT_AVAILABLE";
    public const string MarketPriceStale = "MARKET_PRICE_STALE";

    public const string PositionInsufficientQuantity =
        "POSITION_INSUFFICIENT_QUANTITY";
        
    public const string PositionNotFound =
        "POSITION_NOT_FOUND";

    public const string PositionConcurrencyConflict =
        "POSITION_CONCURRENCY_CONFLICT";

    public const string PortfolioNotFound =
        "PORTFOLIO_NOT_FOUND";

    public const string PortfolioAccessDenied =
        "PORTFOLIO_ACCESS_DENIED";

    public const string AuthenticationRequired =
        "AUTHENTICATION_REQUIRED";

    public const string InternalError =
        "INTERNAL_ERROR";

    public const string IdempotencyConflict =
        "IDEMPOTENCY_CONFLICT";
    
    public const string IdempotencyKeyRequired =
        "IDEMPOTENCY_KEY_REQUIRED";
}