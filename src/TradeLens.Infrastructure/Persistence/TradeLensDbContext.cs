using Microsoft.EntityFrameworkCore;
using TradeLens.Domain.Entities;

namespace TradeLens.Infrastructure.Persistence;

public class TradeLensDbContext : DbContext
{
    public TradeLensDbContext(
        DbContextOptions<TradeLensDbContext> options)
        : base(options)
    {
    }

    public DbSet<Instrument> Instruments => Set<Instrument>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Position> Positions => Set<Position>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TradeLensDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}