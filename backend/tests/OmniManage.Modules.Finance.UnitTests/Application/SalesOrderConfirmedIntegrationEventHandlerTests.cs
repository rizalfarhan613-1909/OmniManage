using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Finance.Application.IntegrationEventHandlers;
using OmniManage.Modules.Finance.Contracts.IntegrationEvents;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.UnitTests.TestDoubles;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Finance.UnitTests.Application;

public sealed class SalesOrderConfirmedIntegrationEventHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000931");
    private static readonly Guid SalesOrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000932");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000933");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000934");
    private static readonly Guid ReceivableAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000935");
    private static readonly Guid RevenueAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000936");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenModulesAreActive_PostsInvoiceJournalAndOutboxEvent()
    {
        var eventId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var invoiceLineId = Guid.NewGuid();
        var revenueLineId = Guid.NewGuid();
        var outboxEventId = Guid.NewGuid();

        var invoices = new FakeArInvoiceRepository();
        var journalEntries = new FakeJournalEntryRepository();
        var accounts = new FakeGlAccountRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var outbox = new FakeFinanceOutbox();
        var licenses = new FakeModuleLicenseChecker();

        var handler = CreateHandler(
            invoices,
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            outbox,
            licenses,
            new FixedGuidGenerator(
                ReceivableAccountId,
                RevenueAccountId,
                invoiceId,
                journalId,
                invoiceLineId,
                revenueLineId,
                outboxEventId));

        await handler.HandleAsync(CreateEvent(eventId), CancellationToken.None);

        accounts.Accounts.Should().HaveCount(2);
        accounts.Accounts.Should().ContainSingle(account =>
            account.AccountCode == ChartOfAccounts.AccountsReceivableCode
            && account.Type == GlAccountType.Asset);
        accounts.Accounts.Should().ContainSingle(account =>
            account.AccountCode == ChartOfAccounts.SalesRevenueCode
            && account.Type == GlAccountType.Revenue);

        invoices.Invoices.Should().ContainSingle();
        var invoice = invoices.Invoices[0];
        invoice.Id.Should().Be(invoiceId);
        invoice.InvoiceNumber.Should().Be($"AR-{SalesOrderId:D}".ToUpperInvariant());
        invoice.SalesOrderId.Should().Be(SalesOrderId);
        invoice.CustomerId.Should().Be(CustomerId);
        invoice.TotalAmount.Should().Be(50m);
        invoice.Status.Should().Be(ArInvoiceStatus.Unpaid);

        journalEntries.JournalEntries.Should().ContainSingle();
        var journal = journalEntries.JournalEntries[0];
        journal.Id.Should().Be(journalId);
        journal.EntryNumber.Should().Be($"JE-{SalesOrderId:D}".ToUpperInvariant());
        journal.DebitTotal.Should().Be(50m);
        journal.CreditTotal.Should().Be(50m);
        journal.Lines.Should().HaveCount(2);
        journal.Lines.Should().ContainSingle(line =>
            line.GlAccountId == ReceivableAccountId
            && line.Debit == 50m
            && line.Credit == 0m);
        journal.Lines.Should().ContainSingle(line =>
            line.GlAccountId == RevenueAccountId
            && line.Debit == 0m
            && line.Credit == 50m);

        outbox.Events.Should().ContainSingle();
        var postedEvent = outbox.Events[0].Should().BeOfType<ArInvoicePostedIntegrationEvent>().Subject;
        postedEvent.EventId.Should().Be(outboxEventId);
        postedEvent.TenantId.Should().Be(TenantId);
        postedEvent.ArInvoiceId.Should().Be(invoiceId);
        postedEvent.SalesOrderId.Should().Be(SalesOrderId);
        postedEvent.CustomerId.Should().Be(CustomerId);
        postedEvent.TotalAmount.Should().Be(50m);
        postedEvent.OccurredOnUtc.Should().Be(Now.UtcDateTime);

        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].EventId.Should().Be(eventId);
        inbox.Entries[0].Status.Should().Be("Processed");
        unitOfWork.SaveCount.Should().Be(1);
        licenses.Checks.Should().Equal(
            (TenantId, ModuleCodes.FinAr),
            (TenantId, ModuleCodes.FinGl));
    }

    [Fact]
    public async Task Handle_WhenControlAccountsAlreadyExist_ReusesThem()
    {
        var invoices = new FakeArInvoiceRepository();
        var journalEntries = new FakeJournalEntryRepository();
        var accounts = new FakeGlAccountRepository();
        accounts.Accounts.Add(CreateAccount(
            ReceivableAccountId,
            ChartOfAccounts.AccountsReceivableCode,
            ChartOfAccounts.AccountsReceivableName,
            GlAccountType.Asset));
        accounts.Accounts.Add(CreateAccount(
            RevenueAccountId,
            ChartOfAccounts.SalesRevenueCode,
            ChartOfAccounts.SalesRevenueName,
            GlAccountType.Revenue));

        var handler = CreateHandler(
            invoices,
            journalEntries,
            accounts,
            new FakeFinanceInbox(),
            new FakeFinanceUnitOfWork(),
            new FakeFinanceOutbox(),
            new FakeModuleLicenseChecker(),
            new FixedGuidGenerator(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()));

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        accounts.Accounts.Should().HaveCount(2);
        journalEntries.JournalEntries.Should().ContainSingle();
        journalEntries.JournalEntries[0].Lines
            .Select(line => line.GlAccountId)
            .Should().BeEquivalentTo([ReceivableAccountId, RevenueAccountId]);
    }

    [Fact]
    public async Task Handle_WhenArModuleIsDisabled_MarksSkippedWithoutPosting()
    {
        var invoices = new FakeArInvoiceRepository();
        var journalEntries = new FakeJournalEntryRepository();
        var accounts = new FakeGlAccountRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var outbox = new FakeFinanceOutbox();
        var licenses = CreateLicenseChecker(ModuleCodes.FinGl);

        var handler = CreateHandler(
            invoices,
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            outbox,
            licenses,
            new FixedGuidGenerator());

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        invoices.Invoices.Should().BeEmpty();
        journalEntries.JournalEntries.Should().BeEmpty();
        accounts.Accounts.Should().BeEmpty();
        outbox.Events.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("SkippedModuleDisabled");
        unitOfWork.SaveCount.Should().Be(1);
        licenses.Checks.Should().Equal((TenantId, ModuleCodes.FinAr));
    }

    [Fact]
    public async Task Handle_WhenGlModuleIsDisabled_MarksSkippedWithoutPosting()
    {
        var invoices = new FakeArInvoiceRepository();
        var journalEntries = new FakeJournalEntryRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var outbox = new FakeFinanceOutbox();
        var licenses = CreateLicenseChecker(ModuleCodes.FinAr);

        var handler = CreateHandler(
            invoices,
            journalEntries,
            new FakeGlAccountRepository(),
            inbox,
            unitOfWork,
            outbox,
            licenses,
            new FixedGuidGenerator());

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        invoices.Invoices.Should().BeEmpty();
        journalEntries.JournalEntries.Should().BeEmpty();
        outbox.Events.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("SkippedModuleDisabled");
        unitOfWork.SaveCount.Should().Be(1);
        licenses.Checks.Should().Equal(
            (TenantId, ModuleCodes.FinAr),
            (TenantId, ModuleCodes.FinGl));
    }

    [Fact]
    public async Task Handle_WhenEventWasAlreadyProcessed_DoesNothing()
    {
        var invoices = new FakeArInvoiceRepository();
        var journalEntries = new FakeJournalEntryRepository();
        var inbox = new FakeFinanceInbox { AlreadyProcessed = true };
        var unitOfWork = new FakeFinanceUnitOfWork();
        var outbox = new FakeFinanceOutbox();
        var licenses = CreateLicenseChecker();

        var handler = CreateHandler(
            invoices,
            journalEntries,
            new FakeGlAccountRepository(),
            inbox,
            unitOfWork,
            outbox,
            licenses,
            new FixedGuidGenerator());

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        invoices.Invoices.Should().BeEmpty();
        journalEntries.JournalEntries.Should().BeEmpty();
        outbox.Events.Should().BeEmpty();
        inbox.Entries.Should().BeEmpty();
        unitOfWork.SaveCount.Should().Be(0);
        licenses.Checks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenInvoiceAlreadyExistsForSalesOrder_MarksProcessedWithoutNewPostings()
    {
        var invoices = new FakeArInvoiceRepository();
        invoices.Invoices.Add(CreateInvoice());
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var outbox = new FakeFinanceOutbox();

        var handler = CreateHandler(
            invoices,
            new FakeJournalEntryRepository(),
            new FakeGlAccountRepository(),
            inbox,
            unitOfWork,
            outbox,
            CreateLicenseChecker(ModuleCodes.FinAr, ModuleCodes.FinGl),
            new FixedGuidGenerator());

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        invoices.Invoices.Should().ContainSingle();
        outbox.Events.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("Processed");
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenEventHasNoLines_MarksFailedAndThrows()
    {
        var invoices = new FakeArInvoiceRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();

        var handler = CreateHandler(
            invoices,
            new FakeJournalEntryRepository(),
            new FakeGlAccountRepository(),
            inbox,
            unitOfWork,
            new FakeFinanceOutbox(),
            CreateLicenseChecker(ModuleCodes.FinAr, ModuleCodes.FinGl),
            new FixedGuidGenerator());

        var integrationEvent = new SalesOrderConfirmedIntegrationEvent(
            Guid.NewGuid(),
            TenantId,
            Now.UtcDateTime,
            SalesOrderId,
            CustomerId,
            []);

        Func<Task> act = () => handler.HandleAsync(integrationEvent, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*positive total*");
        invoices.Invoices.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("Failed");
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenLinesAreNull_MarksFailedAndThrows()
    {
        var invoices = new FakeArInvoiceRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();

        var handler = CreateHandler(
            invoices,
            new FakeJournalEntryRepository(),
            new FakeGlAccountRepository(),
            inbox,
            unitOfWork,
            new FakeFinanceOutbox(),
            CreateLicenseChecker(ModuleCodes.FinAr, ModuleCodes.FinGl),
            new FixedGuidGenerator());

        var integrationEvent = new SalesOrderConfirmedIntegrationEvent(
            Guid.NewGuid(),
            TenantId,
            Now.UtcDateTime,
            SalesOrderId,
            CustomerId,
            null!);

        Func<Task> act = () => handler.HandleAsync(integrationEvent, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*positive total*");
        invoices.Invoices.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("Failed");
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenLineTotalIsNegative_MarksFailedAndThrows()
    {
        var invoices = new FakeArInvoiceRepository();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();

        var handler = CreateHandler(
            invoices,
            new FakeJournalEntryRepository(),
            new FakeGlAccountRepository(),
            inbox,
            unitOfWork,
            new FakeFinanceOutbox(),
            CreateLicenseChecker(ModuleCodes.FinAr, ModuleCodes.FinGl),
            new FixedGuidGenerator());

        var integrationEvent = CreateEvent(
            Guid.NewGuid(),
            new SalesOrderConfirmedLine(ItemId, 1m, 10m, -10m));

        Func<Task> act = () => handler.HandleAsync(integrationEvent, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*positive total*");
        invoices.Invoices.Should().BeEmpty();
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

    private static SalesOrderConfirmedIntegrationEventHandler CreateHandler(
        FakeArInvoiceRepository invoices,
        FakeJournalEntryRepository journalEntries,
        FakeGlAccountRepository accounts,
        FakeFinanceInbox inbox,
        FakeFinanceUnitOfWork unitOfWork,
        FakeFinanceOutbox outbox,
        FakeModuleLicenseChecker licenses,
        FixedGuidGenerator guidGenerator) =>
        new(
            invoices,
            journalEntries,
            accounts,
            inbox,
            unitOfWork,
            outbox,
            licenses,
            guidGenerator,
            new FixedTimeProvider(Now),
            NullLogger<SalesOrderConfirmedIntegrationEventHandler>.Instance);

    private static GlAccount CreateAccount(
        Guid id,
        string code,
        string name,
        GlAccountType type) =>
        GlAccount.Create(id, TenantId, code, name, type, isActive: true, Now, Guid.NewGuid()).Value;

    private static ArInvoice CreateInvoice() =>
        ArInvoice.Create(
            Guid.NewGuid(),
            TenantId,
            ArInvoice.BuildInvoiceNumber(SalesOrderId),
            SalesOrderId,
            CustomerId,
            50m,
            Now,
            Guid.NewGuid()).Value;

    private static SalesOrderConfirmedIntegrationEvent CreateEvent(
        Guid eventId,
        params SalesOrderConfirmedLine[] lines)
    {
        IReadOnlyList<SalesOrderConfirmedLine> eventLines = lines.Length == 0
            ?
            [
                new SalesOrderConfirmedLine(ItemId, 3m, 10m, 30m),
                new SalesOrderConfirmedLine(ItemId, 2m, 10m, 20m),
            ]
            : lines;

        return new SalesOrderConfirmedIntegrationEvent(
            eventId,
            TenantId,
            Now.UtcDateTime,
            SalesOrderId,
            CustomerId,
            eventLines);
    }
}
