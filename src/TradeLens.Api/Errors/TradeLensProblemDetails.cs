using Microsoft.AspNetCore.Mvc;

namespace TradeLens.Api.Errors;

public sealed class TradeLensProblemDetails : ProblemDetails
{
    public string Code { get; set; } = string.Empty;

    public string? TraceId { get; set; }

    public string? CorrelationId { get; set; }

    public IDictionary<string, string[]>? Errors { get; set; }
}
