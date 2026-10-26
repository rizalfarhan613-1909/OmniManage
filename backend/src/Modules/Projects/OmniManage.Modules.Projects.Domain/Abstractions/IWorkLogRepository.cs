using OmniManage.Modules.Projects.Domain.WorkLogs;

namespace OmniManage.Modules.Projects.Domain.Abstractions;

public interface IWorkLogRepository
{
    Task AddAsync(WorkLog workLog, CancellationToken cancellationToken = default);
}
