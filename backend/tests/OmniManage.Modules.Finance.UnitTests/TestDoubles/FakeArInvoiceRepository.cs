using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Ar;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class FakeArInvoiceRepository : IArInvoiceRepository
{
    public List<ArInvoice> Invoices { get; } = [];

    public ArInvoiceListCriteria? LastCriteria { get; private set; }

    public Task AddAsync(ArInvoice invoice, CancellationToken cancellationToken = default)
    {
        Invoices.Add(invoice);
        return Task.CompletedTask;
    }

    public Task<ArInvoice?> GetBySalesOrderIdAsync(
        Guid salesOrderId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(
            Invoices.SingleOrDefault(invoice => invoice.SalesOrderId == salesOrderId));

    public Task<(IReadOnlyList<ArInvoice> Invoices, long TotalItems)> ListAsync(
        ArInvoiceListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;

        IReadOnlyList<ArInvoice> invoices = Invoices
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToArray();

        return Task.FromResult((invoices, (long)Invoices.Count));
    }
}
