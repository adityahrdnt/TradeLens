using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TradeLens.Application.Interfaces;

namespace TradeLens.Application.Tests.Fakes;

public sealed class FakeTransactionRequestHasher
    : ITransactionRequestHasher
{
    public string ComputeHash(
        Guid portfolioId,
        Guid brokerAccountId,
        Guid instrumentId,
        string type,
        long quantity,
        decimal price,
        decimal fee,
        DateOnly transactionDate,
        long sequence)
    {
        var canonicalRequest = string.Join(
            "|",
            portfolioId.ToString("D"),
            brokerAccountId.ToString("D"),
            instrumentId.ToString("D"),
            type,
            quantity.ToString(CultureInfo.InvariantCulture),
            price.ToString("G29", CultureInfo.InvariantCulture),
            fee.ToString("G29", CultureInfo.InvariantCulture),
            transactionDate.ToString("yyyy-MM-dd"),
            sequence.ToString(CultureInfo.InvariantCulture));

        var bytes = Encoding.UTF8.GetBytes(canonicalRequest);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}