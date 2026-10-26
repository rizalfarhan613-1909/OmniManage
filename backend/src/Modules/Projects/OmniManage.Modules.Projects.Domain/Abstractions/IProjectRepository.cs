namespace OmniManage.Modules.Projects.Domain.Abstractions;

public interface IProjectRepository
{
    Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
}
