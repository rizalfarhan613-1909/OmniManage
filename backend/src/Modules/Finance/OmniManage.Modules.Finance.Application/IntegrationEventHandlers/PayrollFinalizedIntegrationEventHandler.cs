using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Hris.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Finance.Application.IntegrationEventHandlers;

/// <summary>
/// UC-05: consumes <see cref="PayrollFinalizedIntegrationEvent"/> and posts a
/// balanced payroll journal (Dr Payroll Expense, Cr Cash/Bank, Cr Tax Payable,
/// Cr Payroll Deductions Payable). FIN_GL is a soft dependency of HRIS_PAYROLL:
/// when it is OFF the event is recorded as SKIPPED_MODULE_DISABLED and the
/// payroll run stays finalized.
/// </summary>
public sealed class PayrollFinalizedIntegrationEventHandler(
    IJournalEntryRepository journalEntries,
    IGlAccountRepository accounts,
    IFinanceInbox inbox,
    IFinanceUnitOfWork unitOfWork,
    IModuleLicenseChecker licenses,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<PayrollFinalizedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PayrollFinalizedIntegrationEvent>
{
    private const string EventTypeName = nameof(PayrollFinalizedIntegrationEvent);

    public string RequiredModuleCode => ModuleCodes.FinGl;

    private string ConsumerName => GetType().FullName ?? GetType().Name;

    public async Task HandleAsync(
        PayrollFinalizedIntegrationEvent integrationEvent,
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

        if (!await licenses
                .IsActiveAsync(integrationEvent.TenantId, RequiredModuleCode, cancellationToken)
                .ConfigureAwait(false))
        {
            await MarkSkippedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Skipped integration event {EventId} of type {EventType} for consumer {Consumer}: module {ModuleCode} is disabled",
                integrationEvent.EventId,
                EventTypeName,
                ConsumerName,
                RequiredModuleCode);

            return;
        }

        if (!TryCreatePosting(integrationEvent, out var posting))
        {
            await MarkFailedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            throw new InvalidOperationException(
                "A finalized payroll event must contain non-negative amounts where net pay plus PPh 21 does not exceed basic salary plus allowances.");
        }

        await PostJournalAsync(integrationEvent, posting, now, cancellationToken).ConfigureAwait(false);
        await MarkProcessedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Processed integration event {EventId} of type {EventType} for consumer {Consumer} for payroll run {PayrollRunId} period {PeriodCode}",
            integrationEvent.EventId,
            EventTypeName,
            ConsumerName,
            integrationEvent.PayrollRunId,
            integrationEvent.PeriodCode);
    }

    private async Task PostJournalAsync(
        PayrollFinalizedIntegrationEvent integrationEvent,
        PayrollPosting posting,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var expense = await EnsureAccountAsync(
                integrationEvent.TenantId,
                ChartOfAccounts.PayrollExpenseCode,
                ChartOfAccounts.PayrollExpenseName,
                GlAccountType.Expense,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        var cash = await EnsureAccountAsync(
                integrationEvent.TenantId,
                ChartOfAccounts.CashAndBankCode,
                ChartOfAccounts.CashAndBankName,
                GlAccountType.Asset,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        var tax = await EnsureAccountAsync(
                integrationEvent.TenantId,
                ChartOfAccounts.TaxPayableCode,
                ChartOfAccounts.TaxPayableName,
                GlAccountType.Liability,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        var deductions = await EnsureAccountAsync(
                integrationEvent.TenantId,
                ChartOfAccounts.PayrollDeductionsPayableCode,
                ChartOfAccounts.PayrollDeductionsPayableName,
                GlAccountType.Liability,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        var lines = new List<JournalEntryLineInput>
        {
            new(guidGenerator.NewGuid(), expense.Id, posting.GrossAmount, 0m),
        };

        if (posting.NetPay > 0m)
        {
            lines.Add(new JournalEntryLineInput(
                guidGenerator.NewGuid(),
                cash.Id,
                0m,
                posting.NetPay));
        }

        if (posting.TaxPayable > 0m)
        {
            lines.Add(new JournalEntryLineInput(
                guidGenerator.NewGuid(),
                tax.Id,
                0m,
                posting.TaxPayable));
        }

        if (posting.DeductionsPayable > 0m)
        {
            lines.Add(new JournalEntryLineInput(
                guidGenerator.NewGuid(),
                deductions.Id,
                0m,
                posting.DeductionsPayable));
        }

        var journalCreated = JournalEntry.Create(
            guidGenerator.NewGuid(),
            integrationEvent.TenantId,
            $"PAYROLL-{integrationEvent.PayrollRunId:D}",
            now,
            $"Payroll journal for period {integrationEvent.PeriodCode} (run {integrationEvent.PayrollRunId:D})",
            lines,
            now,
            FinanceSystemActor.UserId);

        if (journalCreated.IsFailure)
        {
            throw new InvalidOperationException(journalCreated.Error.Message);
        }

        await journalEntries.AddAsync(journalCreated.Value, cancellationToken).ConfigureAwait(false);
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
        PayrollFinalizedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkProcessedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);

    private Task MarkSkippedAsync(
        PayrollFinalizedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkSkippedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);

    private Task MarkFailedAsync(
        PayrollFinalizedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkFailedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);

    private static bool TryCreatePosting(
        PayrollFinalizedIntegrationEvent integrationEvent,
        out PayrollPosting posting)
    {
        posting = default;

        if (integrationEvent.TotalBasicSalary < 0m
            || integrationEvent.TotalAllowances < 0m
            || integrationEvent.TotalDeductions < 0m
            || integrationEvent.TotalKasbonDeducted < 0m
            || integrationEvent.TotalTaxPPh21 < 0m
            || integrationEvent.NetPayTotal < 0m)
        {
            return false;
        }

        decimal grossAmount;
        decimal netPay;
        decimal taxPayable;

        try
        {
            grossAmount = decimal.Round(
                integrationEvent.TotalBasicSalary + integrationEvent.TotalAllowances,
                4,
                MidpointRounding.AwayFromZero);
            netPay = decimal.Round(
                integrationEvent.NetPayTotal,
                4,
                MidpointRounding.AwayFromZero);
            taxPayable = decimal.Round(
                integrationEvent.TotalTaxPPh21,
                4,
                MidpointRounding.AwayFromZero);
        }
        catch (OverflowException)
        {
            return false;
        }

        if (grossAmount <= 0m || netPay > grossAmount || taxPayable > grossAmount - netPay)
        {
            return false;
        }

        // Kasbon and other deductions are grouped into the deductions payable
        // control account; the journal must stay balanced against gross pay.
        var deductionsPayable = decimal.Round(
            grossAmount - netPay - taxPayable,
            4,
            MidpointRounding.AwayFromZero);

        posting = new PayrollPosting(grossAmount, netPay, taxPayable, deductionsPayable);
        return true;
    }

    private readonly record struct PayrollPosting(
        decimal GrossAmount,
        decimal NetPay,
        decimal TaxPayable,
        decimal DeductionsPayable);
}
