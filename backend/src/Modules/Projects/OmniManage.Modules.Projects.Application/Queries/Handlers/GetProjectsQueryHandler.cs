using MediatR;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.Projects;

namespace OmniManage.Modules.Projects.Application.Queries;

public sealed class GetProjectsQueryHandler(IProjectRepository projects)
    : IRequestHandler<GetProjectsQuery, PagedResult<ProjectDto>>
{
    private static readonly IReadOnlyDictionary<string, ProjectSortField> SortFields =
        new Dictionary<string, ProjectSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = ProjectSortField.Code,
            ["name"] = ProjectSortField.Name,
            ["startDate"] = ProjectSortField.StartDate,
        };

    public async Task<PagedResult<ProjectDto>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ProjectStatus? status = null;

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<ProjectStatus>(request.Status, ignoreCase: true, out var parsedStatus)
            && Enum.IsDefined(parsedStatus))
        {
            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, ProjectSortField.Code)
            .Select(option => new ProjectSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new ProjectListCriteria(
            skip,
            pageSize,
            request.Search?.Trim(),
            request.CustomerId,
            status,
            sort);

        var (found, totalItems) = await projects
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(ProjectDto.From).ToArray();

        return new PagedResult<ProjectDto>(dtos, page, pageSize, totalItems);
    }
}
