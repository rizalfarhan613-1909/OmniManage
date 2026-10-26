using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.Tasks;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Repositories;

public sealed class ProjectTaskRepository(ProjectsDbContext dbContext) : IProjectTaskRepository
{
    public Task AddAsync(ProjectTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        return dbContext.Tasks.AddAsync(task, cancellationToken).AsTask();
    }

    public Task<ProjectTask?> GetForUpdateAsync(
        Guid taskId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == taskId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
