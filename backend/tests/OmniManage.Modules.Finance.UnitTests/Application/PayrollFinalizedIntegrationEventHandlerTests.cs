using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Finance.Application.IntegrationEventHandlers;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.UnitTests.TestDoubles;
using OmniManage.Modules.Hris.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Finance.UnitTests.Application;

public sealed class PayrollFinalizedIntegrationEventHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000941");
    private static readonly Guid PayrollRunId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000942");
    private static readonly Guid ExpenseAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000943");
    private static readonly Guid CashAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000944");
    private static readonly Guid TaxAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000945");
    private static readonly Guid DeductionsAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000946");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenGlIsActive_PostsBalancedPayrollJournal()
    {
        var eventId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var expenseLineId = Guid.NewGuid();
        var cashLineId = Guid.NewGuid();
        var taxLineId = Guid.NewGuid();
        var deductionsLineId = Guid.NewGuid();

        var journalEntries = new FakeJournalEntryRepository();
        var accounts = new FakeGlAccountRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var licenses = CreateLicenseChecker(ModuleCodes.FinGl);

        var handler = CreateHandler(
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            licenses,
            new FixedGuidGenerator(
                ExpenseAccountId,
                CashAccountId,
                TaxAccountId,
                DeductionsAccountId,
                expenseLineId,
                cashLineId,
                taxLineId,
                deductionsLineId,
                journalId));

        await handler.HandleAsync(CreateEvent(eventId), CancellationToken.None);

        accounts.Accounts.Should().HaveCount(4);
        accounts.Accounts.Should().ContainSingle(account =>
            account.AccountCode == ChartOfAccounts.PayrollExpenseCode
            && account.Type == GlAccountType.Expense);
        accounts.Accounts.Should().ContainSingle(account =>
            account.AccountCode == ChartOfAccounts.CashAndBankCode
            && account.Type == GlAccountType.Asset);
        accounts.Accounts.Should().ContainSingle(account =>
            account.AccountCode == ChartOfAccounts.TaxPayableCode
            && account.Type == GlAccountType.Liability);
        accounts.Accounts.Should().ContainSingle(account =>
            account.AccountCode == ChartOfAccounts.PayrollDeductionsPayableCode
            && account.Type == GlAccountType.Liability);

        journalEntries.JournalEntries.Should().ContainSingle();
        var journal = journalEntries.JournalEntries[0];
        journal.Id.Should().Be(journalId);
        journal.EntryNumber.Should().Be($"PAYROLL-{PayrollRunId:D}".ToUpperInvariant());
        journal.Description.Should().Contain("2026-10");
        journal.DebitTotal.Should().Be(12_000m);
        journal.CreditTotal.Should().Be(12_000m);
        journal.Lines.Should().HaveCount(4);
        journal.Lines.Should().ContainSingle(line =>
            line.GlAccountId == ExpenseAccountId
            && line.Debit == 12_000m
            && line.Credit == 0m);
        journal.Lines.Should().ContainSingle(line =>
            line.GlAccountId == CashAccountId
            && line.Debit == 0m
            && line.Credit == 10_500m);
        journal.Lines.Should().ContainSingle(line =>
            line.GlAccountId == TaxAccountId
            && line.Debit == 0m
            && line.Credit == 500m);
        journal.Lines.Should().ContainSingle(line =>
            line.GlAccountId == DeductionsAccountId
            && line.Debit == 0m
            && line.Credit == 1_000m);

        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].EventId.Should().Be(eventId);
        inbox.Entries[0].Status.Should().Be("Processed");
        unitOfWork.SaveCount.Should().Be(1);
        licenses.Checks.Should().Equal((TenantId, ModuleCodes.FinGl));
    }

    [Fact]
    public async Task Handle_WhenControlAccountsAlreadyExist_ReusesThem()
    {
        var accounts = new FakeGlAccountRepository();
        accounts.Accounts.Add(CreateAccount(
            ExpenseAccountId,
            ChartOfAccounts.PayrollExpenseCode,
            ChartOfAccounts.PayrollExpenseName,
            GlAccountType.Expense));
        accounts.Accounts.Add(CreateAccount(
            CashAccountId,
            ChartOfAccounts.CashAndBankCode,
            ChartOfAccounts.CashAndBankName,
            GlAccountType.Asset));
        accounts.Accounts.Add(CreateAccount(
            TaxAccountId,
            ChartOfAccounts.TaxPayableCode,
            ChartOfAccounts.TaxPayableName,
            GlAccountType.Liability));
        accounts.Accounts.Add(CreateAccount(
            DeductionsAccountId,
            ChartOfAccounts.PayrollDeductionsPayableCode,
            ChartOfAccounts.PayrollDeductionsPayableName,
            GlAccountType.Liability));

        var journalEntries = new FakeJournalEntryRepository();
        var handler = CreateHandler(
            journalEntries,
            accounts,
            new FakeFinanceInbox(),
            new FakeFinanceUnitOfWork(),
            CreateLicenseChecker(ModuleCodes.FinGl),
            new FixedGuidGenerator(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()));

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        accounts.Accounts.Should().HaveCount(4);
        journalEntries.JournalEntries.Should().ContainSingle();
        journalEntries.JournalEntries[0].Lines
            .Select(line => line.GlAccountId)
            .Should().BeEquivalentTo(
                [ExpenseAccountId, CashAccountId, TaxAccountId, DeductionsAccountId]);
    }

    [Fact]
    public async Task Handle_WhenTaxIsZero_OmitsTaxLineButStaysBalanced()
    {
        var journalEntries = new FakeJournalEntryRepository();
        var handler = CreateHandler(
            journalEntries,
            new FakeGlAccountRepository(),
            new FakeFinanceInbox(),
            new FakeFinanceUnitOfWork(),
            CreateLicenseChecker(ModuleCodes.FinGl),
            new FixedGuidGenerator(
                ExpenseAccountId,
                CashAccountId,
                TaxAccountId,
                DeductionsAccountId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()));

        var integrationEvent = CreateEvent(Guid.NewGuid()) with
        {
            TotalTaxPPh21 = 0m,
            NetPayTotal = 11_000m,
        };

        await handler.HandleAsync(integrationEvent, CancellationToken.None);

        journalEntries.JournalEntries.Should().ContainSingle();
        var journal = journalEntries.JournalEntries[0];
        journal.DebitTotal.Should().Be(12_000m);
        journal.CreditTotal.Should().Be(12_000m);
        journal.Lines.Should().HaveCount(3);
        journal.Lines.Should().NotContain(line => line.GlAccountId == TaxAccountId);
    }

    [Fact]
    public async Task Handle_WhenGlModuleIsDisabled_MarksSkippedWithoutPosting()
    {
        var journalEntries = new FakeJournalEntryRepository();
        var accounts = new FakeGlAccountRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var licenses = CreateLicenseChecker();

        var handler = CreateHandler(
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            licenses,
            new FixedGuidGenerator());

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        journalEntries.JournalEntries.Should().BeEmpty();
        accounts.Accounts.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("SkippedModuleDisabled");
        unitOfWork.SaveCount.Should().Be(1);
        licenses.Checks.Should().Equal((TenantId, ModuleCodes.FinGl));
    }

    [Fact]
    public async Task Handle_WhenEventWasAlreadyProcessed_DoesNothing()
    {
        var journalEntries = new FakeJournalEntryRepository();
        var accounts = new FakeGlAccountRepository();
        var inbox = new FakeFinanceInbox { AlreadyProcessed = true };
        var unitOfWork = new FakeFinanceUnitOfWork();
        var licenses = CreateLicenseChecker();

        var handler = CreateHandler(
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            licenses,
            new FixedGuidGenerator());

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        journalEntries.JournalEntries.Should().BeEmpty();
        accounts.Accounts.Should().BeEmpty();
        inbox.Entries.Should().BeEmpty();
        unitOfWork.SaveCount.Should().Be(0);
        licenses.Checks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNetPayExceedsGrossPay_MarksFailedAndThrows()
    {
        var journalEntries = new FakeJournalEntryRepository();
        var accounts = new FakeGlAccountRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();

        var handler = CreateHandler(
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            CreateLicenseChecker(ModuleCodes.FinGl),
            new FixedGuidGenerator());

        var integrationEvent = CreateEvent(Guid.NewGuid()) with
        {
            NetPayTotal = 13_000m,
        };

        Func<Task> act = () => handler.HandleAsync(integrationEvent, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*non-negative amounts*");
        journalEntries.JournalEntries.Should().BeEmpty();
        accounts.Accounts.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("Failed");
        unitOfWork.SaveCount.Should().Be(1);
    }

    private static FakeModuleLicenseChecker CreateLicenseChecker(params string[] activeModules)
    {
        var checker = new FakeModuleLicenseChecker();
        checker.ActiveModules.Clear();

        foreach (var moduleCode in activeModules)
        {
            checker.ActiveModules.Add(moduleCode);
        }

        return checker;
    }

    private static PayrollFinalizedIntegrationEventHandler CreateHandler(
        FakeJournalEntryRepository journalEntries,
        FakeGlAccountRepository accounts,
        FakeFinanceInbox inbox,
        FakeFinanceUnitOfWork unitOfWork,
        FakeModuleLicenseChecker licenses,
        FixedGuidGenerator guidGenerator) =>
        new(
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            licenses,
            guidGenerator,
            new FixedTimeProvider(Now),
            NullLogger<PayrollFinalizedIntegrationEventHandler>.Instance);

    private static GlAccount CreateAccount(
        Guid id,
        string code,
        string name,
        GlAccountType type) =>
        GlAccount.Create(id, TenantId, code, name, type, isActive: true, Now, Guid.NewGuid()).Value;

    private static PayrollFinalizedIntegrationEvent CreateEvent(Guid eventId) =>
        new(
            eventId,
            TenantId,
            Now.UtcDateTime,
            PayrollRunId,
            "2026-10",
            TotalBasicSalary: 10_000m,
            TotalAllowances: 2_000m,
            TotalDeductions: 1_000m,
            TotalKasbonDeducted: 0m,
            TotalTaxPPh21: 500m,
            NetPayTotal: 10_500m);
}
