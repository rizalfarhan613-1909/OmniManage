using MediatR;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.Tasks;

namespace OmniManage.Modules.Projects.Application.Queries;

public sealed class GetProjectTasksQueryHandler(IProjectTaskRepository tasks)
    : IRequestHandler<GetProjectTasksQuery, PagedResult<TaskDto>>
{
    private static readonly IReadOnlyDictionary<string, ProjectTaskSortField> SortFields =
        new Dictionary<string, ProjectTaskSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = ProjectTaskSortField.Title,
            ["status"] = ProjectTaskSortField.Status,
            ["estimatedHours"] = ProjectTaskSortField.EstimatedHours,
        };

    public async Task<PagedResult<TaskDto>> Handle(
        GetProjectTasksQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ProjectTaskStatus? status = null;

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<ProjectTaskStatus>(request.Status, ignoreCase: true, out var parsedStatus)
            && Enum.IsDefined(parsedStatus))
        {
            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, ProjectTaskSortField.Title)
            .Select(option => new ProjectTaskSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new ProjectTaskListCriteria(
            skip,
            pageSize,
            request.Search?.Trim(),
            request.ProjectId,
            request.AssigneeEmployeeId,
            status,
            sort);

        var (found, totalItems) = await tasks
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(TaskDto.From).ToArray();

        return new PagedResult<TaskDto>(dtos, page, pageSize, totalItems);
    }
}
