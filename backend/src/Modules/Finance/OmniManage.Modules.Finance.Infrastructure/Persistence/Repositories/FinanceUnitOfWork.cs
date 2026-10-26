using OmniManage.Modules.Finance.Domain.Abstractions;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class FinanceUnitOfWork(FinanceDbContext dbContext) : IFinanceUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
