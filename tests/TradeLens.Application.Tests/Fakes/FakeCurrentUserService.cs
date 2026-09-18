using TradeLens.Application.Interfaces;

public sealed class FakeCurrentUserService
        : ICurrentUserService
    {
        public FakeCurrentUserService(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }