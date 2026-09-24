using System.Net.Http;

namespace TradeLens.Integration.Tests.Fakes;

public sealed class FakeHttpClientFactory
    : IHttpClientFactory
{
    private readonly HttpClient _httpClient;

    public FakeHttpClientFactory(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }
}