using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Provisions the chart of accounts and derives A/R invoices plus double-entry
/// postings from confirmed sales orders. Two invoices per month are marked as
/// paid so cash-flow and open A/R metrics both have data.
/// </summary>
internal static class FinanceDataSeeder
{
    private static readonly GlAccountSeed[] Accounts =
    [
        new(ChartOfAccounts.CashAndBankCode, ChartOfAccounts.CashAndBankName, GlAccountType.Asset),
        new(ChartOfAccounts.AccountsReceivableCode, ChartOfAccounts.AccountsReceivableName, GlAccountType.Asset),
        new(ChartOfAccounts.TaxPayableCode, ChartOfAccounts.TaxPayableName, GlAccountType.Liability),
        new(ChartOfAccounts.SalesRevenueCode, ChartOfAccounts.SalesRevenueName, GlAccountType.Revenue),
    ];

    public static async Task SeedAsync(
        FinanceDbContext context,
        Guid tenantId,
        Guid createdBy,
        IReadOnlyList<SeedSalesOrder> salesOrders,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(salesOrders);
        ArgumentNullException.ThrowIfNull(guidGenerator);

        var accountCodes = Accounts.Select(account => account.Code).ToArray();
        var existingAccountCodes = await context.GlAccounts
            .Select(account => account.AccountCode)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownAccountCodes = existingAccountCodes.ToHashSet(StringComparer.Ordinal);

        foreach (var account in Accounts)
        {
            if (knownAccountCodes.Contains(account.Code))
            {
                continue;
            }

            context.GlAccounts.Add(EnsureSuccess(GlAccount.Create(
                guidGenerator.NewGuid(),
                tenantId,
                account.Code,
                account.Name,
                account.Type,
                isActive: true,
                now,
                createdBy)));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var accountsByCode = await context.GlAccounts
            .AsNoTracking()
            .Where(account => accountCodes.Contains(account.AccountCode))
            .ToDictionaryAsync(
                account => account.AccountCode,
                account => account.Id,
                StringComparer.Ordinal,
                cancellationToken)
            .ConfigureAwait(false);

        var confirmedOrders = salesOrders
            .Where(order => order.IsConfirmed)
            .OrderBy(order => order.CreatedAt)
            .ToArray();

        var paidOrderIds = GetPaidOrderIds(confirmedOrders);

        var existingInvoiceNumbers = await context.ArInvoices
            .Select(invoice => invoice.InvoiceNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownInvoiceNumbers = existingInvoiceNumbers.ToHashSet(StringComparer.Ordinal);

        foreach (var order in confirmedOrders)
        {
            var invoiceNumber = ArInvoice.BuildInvoiceNumber(order.Id);

            if (!knownInvoiceNumbers.Add(invoiceNumber))
            {
                continue;
            }

            var invoiceCreatedAt = order.CreatedAt.AddMinutes(90);

            if (invoiceCreatedAt > now)
            {
                invoiceCreatedAt = now;
            }

            var invoice = EnsureSuccess(ArInvoice.Create(
                guidGenerator.NewGuid(),
                tenantId,
                invoiceNumber,
                order.Id,
                order.CustomerId,
                order.TotalAmount,
                invoiceCreatedAt,
                createdBy));

            if (paidOrderIds.Contains(order.Id))
            {
                EnsureSuccess(invoice.MarkPaid(invoiceCreatedAt.AddMinutes(30), createdBy));
            }

            context.ArInvoices.Add(invoice);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var existingEntryNumbers = await context.JournalEntries
            .Select(entry => entry.EntryNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownEntryNumbers = existingEntryNumbers.ToHashSet(StringComparer.Ordinal);

        var receivableAccountId = accountsByCode[ChartOfAccounts.AccountsReceivableCode];
        var revenueAccountId = accountsByCode[ChartOfAccounts.SalesRevenueCode];
        var cashAccountId = accountsByCode[ChartOfAccounts.CashAndBankCode];

        foreach (var order in confirmedOrders)
        {
            var postedAt = order.CreatedAt.AddHours(1);
            var arEntryNumber = $"JE-AR-{order.OrderNumber}";

            if (knownEntryNumbers.Add(arEntryNumber))
            {
                context.JournalEntries.Add(EnsureSuccess(JournalEntry.Create(
                    guidGenerator.NewGuid(),
                    tenantId,
                    arEntryNumber,
                    postedAt,
                    $"A/R invoice for sales order {order.OrderNumber}",
                    [
                        new JournalEntryLineInput(
                            guidGenerator.NewGuid(),
                            receivableAccountId,
                            order.TotalAmount,
                            0m),
                        new JournalEntryLineInput(
                            guidGenerator.NewGuid(),
                            revenueAccountId,
                            0m,
                            order.TotalAmount),
                    ],
                    postedAt,
                    createdBy)));
            }

            if (!paidOrderIds.Contains(order.Id))
            {
                continue;
            }

            var cashEntryNumber = $"JE-CASH-{order.OrderNumber}";

            if (!knownEntryNumbers.Add(cashEntryNumber))
            {
                continue;
            }

            var collectedAt = postedAt.AddHours(2);

            if (collectedAt > now)
            {
                collectedAt = now;
            }

            context.JournalEntries.Add(EnsureSuccess(JournalEntry.Create(
                guidGenerator.NewGuid(),
                tenantId,
                cashEntryNumber,
                collectedAt,
                $"Cash collection for sales order {order.OrderNumber}",
                [
                    new JournalEntryLineInput(
                        guidGenerator.NewGuid(),
                        cashAccountId,
                        order.TotalAmount,
                        0m),
                    new JournalEntryLineInput(
                        guidGenerator.NewGuid(),
                        receivableAccountId,
                        0m,
                        order.TotalAmount),
                ],
                collectedAt,
                createdBy)));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static HashSet<Guid> GetPaidOrderIds(IReadOnlyList<SeedSalesOrder> confirmedOrders)
    {
        var paidOrderIds = new HashSet<Guid>();

        foreach (var monthGroup in confirmedOrders.GroupBy(
            order => new { order.CreatedAt.Year, order.CreatedAt.Month }))
        {
            foreach (var order in monthGroup.OrderBy(order => order.CreatedAt).Take(2))
            {
                paidOrderIds.Add(order.Id);
            }
        }

        return paidOrderIds;
    }

    private static T EnsureSuccess<T>(OmniManage.BuildingBlocks.Domain.Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The finance demo seed could not be created: {result.Error.Code}.");
        }

        return result.Value;
    }

    private static void EnsureSuccess(OmniManage.BuildingBlocks.Domain.Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The finance demo seed could not be created: {result.Error.Code}.");
        }
    }

    private sealed record GlAccountSeed(string Code, string Name, GlAccountType Type);
}
