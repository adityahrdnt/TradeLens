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

            TransactionNotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    TradeLensErrorCode.TransactionNotFound,
                    "Transaction Not Found"
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

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    TradeLensErrorCode.InternalError,
                    "Internal Server Error"
                )
        };

        httpContext.Response.StatusCode = status;

        var problemDetails = new TradeLensProblemDetails
        {
            Status = status,
            Title = title,
            Code = code,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            TraceId = httpContext.TraceIdentifier
        };

        problemDetails.Type = $"https://api.tradelens.com/problems/{code.ToLowerInvariant()}";

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