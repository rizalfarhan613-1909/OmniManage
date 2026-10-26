using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Application.Common;

namespace OmniManage.Modules.Procurement.Application.Suppliers.GetSuppliers;

public sealed record GetSuppliersQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    bool? IsActive = null,
    string? Sort = null) : IQuery<Result<PagedResult<SupplierDto>>>;
