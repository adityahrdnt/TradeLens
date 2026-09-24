using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TradeLens.Application.Exceptions;
using TradeLens.Domain.Exceptions;

namespace TradeLens.Api.Errors;

public sealed class TradeLensExceptionHandler
    : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<TradeLensExceptionHandler> _logger;

    public TradeLensExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<TradeLensExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        var (status, code, title) = exception switch
        {
            PositionInsufficientQuantityException =>
                (
                    StatusCodes.Status400BadRequest,
                    TradeLensErrorCode.PositionInsufficientQuantity,
                    "Insufficient Position Quantity"
                ),
            
            MarketPriceNotAvailableException =>
                (
                    StatusCodes.Status400BadRequest,
                    TradeLensErrorCode.MarketPriceNotAvailable,
                    "Market Price Not Available"
                ),

            MarketPriceStaleException =>
                (
                    StatusCodes.Status400BadRequest,
                    TradeLensErrorCode.MarketPriceStale,
                    "Market Price Stale"
                ),

            TransactionNotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    TradeLensErrorCode.TransactionNotFound,
                    "Transaction Not Found"
                ),
            
            PortfolioNotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    TradeLensErrorCode.PortfolioNotFound,
                    "Portfolio Not Found"
                ),

            PortfolioAccessDeniedException =>
                (
                    StatusCodes.Status403Forbidden,
                    TradeLensErrorCode.PortfolioAccessDenied,
                    "Portfolio Access Denied"
                ),

            PositionNotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    TradeLensErrorCode.PositionNotFound,
                    "Position Not Found"
                ),

            IdempotencyKeyRequiredException =>
                (
                    StatusCodes.Status400BadRequest,
                    TradeLensErrorCode.IdempotencyKeyRequired,
                    "Idempotency Key Required"
                ),

            IdempotencyConflictException =>
                (
                    StatusCodes.Status409Conflict,
                    TradeLensErrorCode.IdempotencyConflict,
                    "Idempotency Conflict"
                ),

            PositionConcurrencyException =>
                (
                    StatusCodes.Status409Conflict,
                    TradeLensErrorCode.PositionConcurrencyConflict,
                    "Position Concurrency Conflict"
                ),
            
            DomainException =>
                (
                    StatusCodes.Status400BadRequest,
                    TradeLensErrorCode.InvalidTransaction,
                    "Business Rule Violation"
                ),

            UnauthorizedAccessException =>
                (
                    StatusCodes.Status401Unauthorized,
                    TradeLensErrorCode.AuthenticationRequired,
                    "Authentication Required"
                ),

            ValidationException =>
                (
                    StatusCodes.Status400BadRequest,
                    TradeLensErrorCode.ValidationError,
                    "Validation Error"
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    TradeLensErrorCode.InternalError,
                    "Internal Server Error"
                )
        };

        IDictionary<string, string[]>? errors = null;

        if (exception is ValidationException validationException)
        {
            errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group =>
                        char.ToLowerInvariant(group.Key[0])
                        + group.Key[1..],
                    group => group
                        .Select(x => x.ErrorMessage)
                        .ToArray());
        }

        httpContext.Response.StatusCode = status;

        var problemDetails = new TradeLensProblemDetails
        {
            Type = $"https://api.tradelens.com/problems/{code.ToLowerInvariant()}",
            Status = status,
            Title = title,
            Code = code,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            TraceId = httpContext.TraceIdentifier,
            Errors = errors
        };

        await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        
        return true;
    }
}