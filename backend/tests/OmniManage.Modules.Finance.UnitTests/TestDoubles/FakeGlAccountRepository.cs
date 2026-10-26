using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class FakeGlAccountRepository : IGlAccountRepository
{
    public List<GlAccount> Accounts { get; } = [];

    public Task<GlAccount?> GetByCodeAsync(
        string accountCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = accountCode.Trim().ToUpperInvariant();

        return Task.FromResult(
            Accounts.SingleOrDefault(account => account.AccountCode == normalizedCode));
    }

    public Task AddAsync(GlAccount account, CancellationToken cancellationToken = default)
    {
        Accounts.Add(account);
        return Task.CompletedTask;
    }
}
