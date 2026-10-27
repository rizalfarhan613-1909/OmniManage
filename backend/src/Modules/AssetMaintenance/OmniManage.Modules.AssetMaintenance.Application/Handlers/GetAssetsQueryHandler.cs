using MediatR;
using OmniManage.Modules.AssetMaintenance.Application.Common;
using OmniManage.Modules.AssetMaintenance.Application.Dtos;
using OmniManage.Modules.AssetMaintenance.Application.Queries;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;

namespace OmniManage.Modules.AssetMaintenance.Application.Handlers;

public sealed class GetAssetsQueryHandler(IAssetRepository assets)
    : IRequestHandler<GetAssetsQuery, PagedResult<AssetDto>>
{
    private static readonly IReadOnlyDictionary<string, AssetSortField> SortFields =
        new Dictionary<string, AssetSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["assetCode"] = AssetSortField.AssetCode,
            ["name"] = AssetSortField.Name,
            ["acquisitionDate"] = AssetSortField.AcquisitionDate,
        };

    public async Task<PagedResult<AssetDto>> Handle(
        GetAssetsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, AssetSortField.AssetCode)
            .Select(option => new AssetSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new AssetListCriteria(
            skip,
            pageSize,
            request.Search?.Trim(),
            request.Category?.Trim(),
            ParseStatus(request.Status),
            sort);

        var (found, totalItems) = await assets
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(AssetDto.From).ToArray();

        return new PagedResult<AssetDto>(dtos, page, pageSize, totalItems);
    }

    private static AssetStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return Enum.TryParse<AssetStatus>(status.Trim(), ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }
}
