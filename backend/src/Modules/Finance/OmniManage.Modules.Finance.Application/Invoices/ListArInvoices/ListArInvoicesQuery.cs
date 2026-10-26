using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Application.Common;

namespace OmniManage.Modules.Finance.Application.Invoices.ListArInvoices;

public sealed record ListArInvoicesQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    string? Status = null,
    string? Sort = null) : IQuery<Result<PagedResult<ArInvoiceDto>>>;
