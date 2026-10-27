using OmniManage.Modules.Projects.Domain.WorkLogs;

namespace OmniManage.Modules.Projects.Domain.Abstractions;

public interface IWorkLogRepository
{
    Task AddAsync(WorkLog workLog, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<WorkLog> WorkLogs, long TotalItems)> ListAsync(
        WorkLogListCriteria criteria,
        CancellationToken cancellationToken = default);
}
