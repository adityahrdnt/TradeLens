using System.Security.Cryptography;
using System.Text;
using TradeLens.Application.Interfaces;

namespace TradeLens.Infrastructure.Services;

public sealed class TransactionRequestHasher
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
            quantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
            price.ToString("G29", System.Globalization.CultureInfo.InvariantCulture),
            fee.ToString("G29", System.Globalization.CultureInfo.InvariantCulture),
            transactionDate.ToString("yyyy-MM-dd"),
            sequence.ToString(System.Globalization.CultureInfo.InvariantCulture));

        var bytes = Encoding.UTF8.GetBytes(canonicalRequest);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}