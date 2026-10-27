using MediatR;
using OmniManage.Modules.AssetMaintenance.Application.Common;
using OmniManage.Modules.AssetMaintenance.Application.Dtos;

namespace OmniManage.Modules.AssetMaintenance.Application.Queries;

public class GetMaintenanceOrdersQuery : IRequest<PagedResult<MaintenanceOrderDto>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public Guid? AssetId { get; set; }

    public string? Status { get; set; }

    public string? Sort { get; set; }
}
