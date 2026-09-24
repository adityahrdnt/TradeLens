using System.Net;
using System.Net.Http;
using System.Text;

namespace TradeLens.Integration.Tests.Fakes;

public sealed class FakeHttpMessageHandler
    : HttpMessageHandler
{
    private readonly string? _responseContent;
    private readonly HttpStatusCode _statusCode;
    
    private readonly Func<
        HttpRequestMessage,
        (HttpStatusCode StatusCode, string Content)>?
        _responseFactory;

    public HttpRequestMessage? LastRequest { get; private set; }

    public List<string> RequestedUrls { get; } = new();

    public FakeHttpMessageHandler(
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseContent = responseContent;
        _statusCode = statusCode;
    }

    public FakeHttpMessageHandler(
        Func<
            HttpRequestMessage,
            (HttpStatusCode StatusCode, string Content)>
            responseFactory)
    {
        _responseFactory = responseFactory;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;

        RequestedUrls.Add(
            request.RequestUri?.ToString() ?? string.Empty);

        var responseData =
            _responseFactory is not null
                ? _responseFactory(request)
                : (
                    StatusCode: _statusCode,
                    Content: _responseContent!);

        var response =
            new HttpResponseMessage(
                responseData.StatusCode)
            {
                Content = new StringContent(
                    responseData.Content,
                    Encoding.UTF8,
                    "application/json")
            };

        return Task.FromResult(response);
    }
}