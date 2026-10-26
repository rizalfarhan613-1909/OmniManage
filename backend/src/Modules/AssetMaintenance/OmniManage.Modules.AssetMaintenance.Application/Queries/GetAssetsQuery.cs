using MediatR;
using OmniManage.Modules.AssetMaintenance.Application.Common;
using OmniManage.Modules.AssetMaintenance.Application.Dtos;

namespace OmniManage.Modules.AssetMaintenance.Application.Queries;

public class GetAssetsQuery : IRequest<PagedResult<AssetDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}