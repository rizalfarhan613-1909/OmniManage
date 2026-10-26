using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.WorkLogs;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Repositories;

public sealed class WorkLogRepository(ProjectsDbContext dbContext) : IWorkLogRepository
{
    public Task AddAsync(WorkLog workLog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workLog);

        return dbContext.WorkLogs.AddAsync(workLog, cancellationToken).AsTask();
    }
}
