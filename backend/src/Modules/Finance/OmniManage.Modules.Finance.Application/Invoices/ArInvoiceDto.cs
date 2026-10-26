using System.Globalization;
using OmniManage.Modules.Finance.Domain.Ar;

namespace OmniManage.Modules.Finance.Application.Invoices;

public sealed record ArInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid SalesOrderId,
    Guid CustomerId,
    string TotalAmount,
    string Status,
    DateTimeOffset CreatedAt)
{
    internal static ArInvoiceDto From(ArInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return new ArInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.SalesOrderId,
            invoice.CustomerId,
            FormatAmount(invoice.TotalAmount),
            invoice.Status.ToString(),
            invoice.CreatedAt);
    }

    internal static string FormatAmount(decimal amount) =>
        amount.ToString("0.0000", CultureInfo.InvariantCulture);
}
