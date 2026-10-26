using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Application.Common;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Errors;

namespace OmniManage.Modules.Finance.Application.Invoices.ListArInvoices;

public sealed class ListArInvoicesQueryHandler(IArInvoiceRepository invoices)
    : IQueryHandler<ListArInvoicesQuery, Result<PagedResult<ArInvoiceDto>>>
{
    private static readonly IReadOnlyDictionary<string, ArInvoiceSortField> SortFields =
        new Dictionary<string, ArInvoiceSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["invoiceNumber"] = ArInvoiceSortField.InvoiceNumber,
            ["status"] = ArInvoiceSortField.Status,
            ["totalAmount"] = ArInvoiceSortField.TotalAmount,
            ["createdAt"] = ArInvoiceSortField.CreatedAt,
        };

    public async Task<Result<PagedResult<ArInvoiceDto>>> Handle(
        ListArInvoicesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        ArInvoiceStatus? status = null;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<ArInvoiceStatus>(query.Status, ignoreCase: true, out var parsedStatus)
                || !Enum.IsDefined(parsedStatus))
            {
                return ArInvoiceErrors.InvalidStatusFilter;
            }

            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, ArInvoiceSortField.CreatedAt)
            .Select(option => new ArInvoiceSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new ArInvoiceListCriteria(skip, pageSize, query.Search?.Trim(), status, sort);

        var (found, totalItems) = await invoices
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(ArInvoiceDto.From).ToArray();

        return new PagedResult<ArInvoiceDto>(dtos, page, pageSize, totalItems);
    }
}
