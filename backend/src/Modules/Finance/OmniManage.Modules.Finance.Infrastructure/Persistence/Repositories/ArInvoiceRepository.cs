using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Ar;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class ArInvoiceRepository(FinanceDbContext dbContext) : IArInvoiceRepository
{
    public Task AddAsync(ArInvoice invoice, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return dbContext.ArInvoices.AddAsync(invoice, cancellationToken).AsTask();
    }

    public Task<ArInvoice?> GetBySalesOrderIdAsync(
        Guid salesOrderId,
        CancellationToken cancellationToken = default) =>
        dbContext.ArInvoices
            .AsNoTracking()
            .FirstOrDefaultAsync(invoice => invoice.SalesOrderId == salesOrderId, cancellationToken);

    public async Task<(IReadOnlyList<ArInvoice> Invoices, long TotalItems)> ListAsync(
        ArInvoiceListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<ArInvoice> query = dbContext.ArInvoices.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(invoice => invoice.InvoiceNumber.ToLower().Contains(term));
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(invoice => invoice.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var invoices = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (invoices, totalItems);
    }

    private static IQueryable<ArInvoice> ApplySort(
        IQueryable<ArInvoice> query,
        ArInvoiceListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new ArInvoiceSortOption(ArInvoiceSortField.CreatedAt, true)
            : criteria.Sort[0];

        IOrderedQueryable<ArInvoice> ordered = option.Field switch
        {
            ArInvoiceSortField.InvoiceNumber => option.Descending
                ? query.OrderByDescending(invoice => invoice.InvoiceNumber)
                : query.OrderBy(invoice => invoice.InvoiceNumber),
            ArInvoiceSortField.Status => option.Descending
                ? query.OrderByDescending(invoice => invoice.Status)
                : query.OrderBy(invoice => invoice.Status),
            ArInvoiceSortField.TotalAmount => option.Descending
                ? query.OrderByDescending(invoice => invoice.TotalAmount)
                : query.OrderBy(invoice => invoice.TotalAmount),
            ArInvoiceSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(invoice => invoice.CreatedAt)
                : query.OrderBy(invoice => invoice.CreatedAt),
            _ => query.OrderByDescending(invoice => invoice.CreatedAt),
        };

        return ordered.ThenBy(invoice => invoice.Id);
    }
}
