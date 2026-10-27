using MediatR;
using OmniManage.Modules.Hris.Application.Common;
using OmniManage.Modules.Hris.Application.Dtos;

namespace OmniManage.Modules.Hris.Application.Queries;

public class GetEmployeesQuery : IRequest<PagedResult<EmployeeDto>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public Guid? DepartmentId { get; set; }

    public bool? IsActive { get; set; }

    public string? Sort { get; set; }
}
