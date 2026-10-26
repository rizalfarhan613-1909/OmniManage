using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class GlAccountRepository(FinanceDbContext dbContext) : IGlAccountRepository
{
    public Task<GlAccount?> GetByCodeAsync(
        string accountCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountCode);

        var normalizedCode = accountCode.Trim().ToUpperInvariant();

        return dbContext.GlAccounts
            .FirstOrDefaultAsync(account => account.AccountCode == normalizedCode, cancellationToken);
    }

    public Task AddAsync(GlAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        return dbContext.GlAccounts.AddAsync(account, cancellationToken).AsTask();
    }
}
