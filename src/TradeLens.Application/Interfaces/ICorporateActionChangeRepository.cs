using TradeLens.Domain.Entities;

namespace TradeLens.Application.Interfaces;

public interface ICorporateActionChangeRepository
{
    Task AddAsync(
        CorporateActionChange change,
        CancellationToken cancellationToken = default);
}
