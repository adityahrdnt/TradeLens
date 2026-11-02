using Microsoft.EntityFrameworkCore;
using TradeLens.Domain.Entities;
using TradeLens.Infrastructure.Persistence;

namespace TradeLens.Integration.Tests.Infrastructure;

public static class IntegrationTestData
{
    private static readonly SemaphoreSlim UserCreationLock = new(1, 1);

    public static async Task EnsureUserExistsAsync(
        TradeLensDbContext dbContext,
        Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        await UserCreationLock.WaitAsync();

        try
        {
            var userExists = await dbContext.Users
                .AnyAsync(x => x.Id == userId);

            if (userExists)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;

            var user = User.CreateRegistration(
                userId,
                $"integration-{userId:N}@tradelens.test",
                $"Integration User {userId:N}",
                "integration-test-password-hash",
                now);

            user.Activate(now);

            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();
        }
        finally
        {
            UserCreationLock.Release();
        }
    }

    public static async Task EnsureUsersExistAsync(
        TradeLensDbContext dbContext,
        params Guid[] userIds)
    {
        foreach (var userId in userIds.Distinct())
        {
            await EnsureUserExistsAsync(dbContext, userId);
        }
    }
}
