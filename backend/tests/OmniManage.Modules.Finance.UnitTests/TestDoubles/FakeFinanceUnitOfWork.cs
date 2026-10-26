using OmniManage.Modules.Finance.Domain.Abstractions;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class FakeFinanceUnitOfWork : IFinanceUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
