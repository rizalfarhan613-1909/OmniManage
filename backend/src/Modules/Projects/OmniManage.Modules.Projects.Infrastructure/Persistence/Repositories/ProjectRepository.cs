using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(ProjectsDbContext dbContext) : IProjectRepository
{
    public Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        dbContext.Projects.AnyAsync(project => project.Id == projectId, cancellationToken);
}
