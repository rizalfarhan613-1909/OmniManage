using OmniManage.Modules.Finance.Domain.Ar;

namespace OmniManage.Modules.Finance.Domain.Abstractions;

public sealed record ArInvoiceListCriteria(
    int Skip,
    int Take,
    string? Search,
    ArInvoiceStatus? Status,
    IReadOnlyList<ArInvoiceSortOption> Sort);

public sealed record ArInvoiceSortOption(ArInvoiceSortField Field, bool Descending);

public enum ArInvoiceSortField
{
    InvoiceNumber = 1,
    Status = 2,
    TotalAmount = 3,
    CreatedAt = 4,
}
