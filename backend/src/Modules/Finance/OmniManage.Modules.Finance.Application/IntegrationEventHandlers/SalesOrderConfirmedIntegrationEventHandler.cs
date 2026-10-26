using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Finance.Application.IntegrationEvents;
using OmniManage.Modules.Finance.Contracts.IntegrationEvents;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Finance.Application.IntegrationEventHandlers;

/// <summary>
/// UC-01: consumes <see cref="SalesOrderConfirmedIntegrationEvent"/> and posts
/// the A/R invoice plus a balanced journal entry (Dr Accounts Receivable,
/// Cr Sales Revenue). FIN_AR and FIN_GL are soft dependencies of SALES_CORE:
/// when either is OFF the event is recorded as SKIPPED_MODULE_DISABLED and the
/// sales order stays confirmed.
/// </summary>
public sealed class SalesOrderConfirmedIntegrationEventHandler(
    IArInvoiceRepository invoices,
    IJournalEntryRepository journalEntries,
    IGlAccountRepository accounts,
    IFinanceInbox inbox,
    IFinanceUnitOfWork unitOfWork,
    IFinanceOutbox outbox,
    IModuleLicenseChecker licenses,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<SalesOrderConfirmedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<SalesOrderConfirmedIntegrationEvent>
{
    private const string EventTypeName = nameof(SalesOrderConfirmedIntegrationEvent);

    // FIN_AR is the primary capability; FIN_GL is a hard dependency of FIN_AR
    // and is checked explicitly below before any posting is attempted.
    public string RequiredModuleCode => ModuleCodes.FinAr;

    private string ConsumerName => GetType().FullName ?? GetType().Name;

    public async Task HandleAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var now = clock.GetUtcNow();

        if (await inbox
                .IsProcessedAsync(integrationEvent.EventId, ConsumerName, cancellationToken)
                .ConfigureAwait(false))
        {
            return;
        }

        if (!await IsPostingEnabledAsync(integrationEvent.TenantId, cancellationToken).ConfigureAwait(false))
        {
            await MarkSkippedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Skipped integration event {EventId} of type {EventType} for consumer {Consumer}: modules {ModuleCode} are disabled",
                integrationEvent.EventId,
                EventTypeName,
                ConsumerName,
                ModuleCodes.FinAr);

            return;
        }

        var existingInvoice = await invoices
            .GetBySalesOrderIdAsync(integrationEvent.SalesOrderId, cancellationToken)
            .ConfigureAwait(false);

        if (existingInvoice is not null)
        {
            await MarkProcessedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Integration event {EventId} for sales order {SalesOrderId} already produced A/R invoice {InvoiceNumber}",
                integrationEvent.EventId,
                integrationEvent.SalesOrderId,
                existingInvoice.InvoiceNumber);

            return;
        }

        if (!TryCalculateTotal(integrationEvent.Lines, out var totalAmount))
        {
            await MarkFailedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(
                "A confirmed sales order event must contain at least one line with a positive total.");
        }

        await PostArInvoiceAsync(integrationEvent, totalAmount, now, cancellationToken).ConfigureAwait(false);
        await MarkProcessedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Processed integration event {EventId} of type {EventType} for consumer {Consumer}",
            integrationEvent.EventId,
            EventTypeName,
            ConsumerName);
    }

    private async Task<bool> IsPostingEnabledAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var arActive = await licenses
            .IsActiveAsync(tenantId, ModuleCodes.FinAr, cancellationToken)
            .ConfigureAwait(false);

        if (!arActive)
        {
            return false;
        }

        return await licenses
            .IsActiveAsync(tenantId, ModuleCodes.FinGl, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task PostArInvoiceAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
        decimal totalAmount,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var receivable = await EnsureAccountAsync(
                integrationEvent.TenantId,
                ChartOfAccounts.AccountsReceivableCode,
                ChartOfAccounts.AccountsReceivableName,
                GlAccountType.Asset,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        var revenue = await EnsureAccountAsync(
                integrationEvent.TenantId,
                ChartOfAccounts.SalesRevenueCode,
                ChartOfAccounts.SalesRevenueName,
                GlAccountType.Revenue,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        var invoiceCreated = ArInvoice.Create(
            guidGenerator.NewGuid(),
            integrationEvent.TenantId,
            ArInvoice.BuildInvoiceNumber(integrationEvent.SalesOrderId),
            integrationEvent.SalesOrderId,
            integrationEvent.CustomerId,
            totalAmount,
            now,
            FinanceSystemActor.UserId);

        if (invoiceCreated.IsFailure)
        {
            throw new InvalidOperationException(invoiceCreated.Error.Message);
        }

        var invoice = invoiceCreated.Value;

        var journalCreated = JournalEntry.Create(
            guidGenerator.NewGuid(),
            integrationEvent.TenantId,
            $"JE-{integrationEvent.SalesOrderId:D}",
            now,
            $"A/R invoice {invoice.InvoiceNumber} for sales order {integrationEvent.SalesOrderId:D}",
            [
                new JournalEntryLineInput(
                    guidGenerator.NewGuid(),
                    receivable.Id,
                    totalAmount,
                    0m),
                new JournalEntryLineInput(
                    guidGenerator.NewGuid(),
                    revenue.Id,
                    0m,
                    totalAmount),
            ],
            now,
            FinanceSystemActor.UserId);

        if (journalCreated.IsFailure)
        {
            throw new InvalidOperationException(journalCreated.Error.Message);
        }

        await invoices.AddAsync(invoice, cancellationToken).ConfigureAwait(false);
        await journalEntries.AddAsync(journalCreated.Value, cancellationToken).ConfigureAwait(false);

        await outbox
            .EnqueueAsync(
                new ArInvoicePostedIntegrationEvent(
                    guidGenerator.NewGuid(),
                    integrationEvent.TenantId,
                    now.UtcDateTime,
                    invoice.Id,
                    invoice.SalesOrderId,
                    invoice.CustomerId,
                    invoice.TotalAmount),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<GlAccount> EnsureAccountAsync(
        Guid tenantId,
        string accountCode,
        string accountName,
        GlAccountType accountType,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await accounts
            .GetByCodeAsync(accountCode, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return existing;
        }

        var created = GlAccount.Create(
            guidGenerator.NewGuid(),
            tenantId,
            accountCode,
            accountName,
            accountType,
            isActive: true,
            now,
            FinanceSystemActor.UserId);

        if (created.IsFailure)
        {
            throw new InvalidOperationException(created.Error.Message);
        }

        await accounts.AddAsync(created.Value, cancellationToken).ConfigureAwait(false);

        return created.Value;
    }

    private Task MarkProcessedAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkProcessedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);

    private Task MarkSkippedAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkSkippedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);

    private Task MarkFailedAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkFailedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);

    private static bool TryCalculateTotal(
        IReadOnlyList<SalesOrderConfirmedLine>? lines,
        out decimal totalAmount)
    {
        totalAmount = 0m;

        if (lines is null || lines.Count == 0)
        {
            return false;
        }

        try
        {
            foreach (var line in lines)
            {
                if (line.LineTotal < 0m)
                {
                    return false;
                }

                totalAmount = decimal.Round(
                    totalAmount + line.LineTotal,
                    4,
                    MidpointRounding.AwayFromZero);
            }
        }
        catch (OverflowException)
        {
            return false;
        }

        return totalAmount > 0m;
    }
}
