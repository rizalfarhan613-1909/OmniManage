using MediatR;
using OmniManage.Modules.Hris.Application.Common;
using OmniManage.Modules.Hris.Application.Dtos;

namespace OmniManage.Modules.Hris.Application.Queries;

public class GetPayrollRunsQuery : IRequest<PagedResult<PayrollRunDto>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public string? PeriodCode { get; set; }

    public string? Status { get; set; }

    public string? Sort { get; set; }
}
