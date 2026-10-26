using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.Infrastructure.Persistence;
using OmniManage.Modules.Finance.UnitTests.TestDoubles;

namespace OmniManage.Modules.Finance.UnitTests.Persistence;

public sealed class FinanceDbContextTests
{
    private static readonly Guid TenantA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000961");
    private static readonly Guid TenantB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000962");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000963");
    private static readonly Guid ReceivableAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000964");
    private static readonly Guid RevenueAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000965");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Model_MapsAggregatesToFinanceSchema()
    {
        using var context = CreateContext(TenantA);

        AssertTable<GlAccount>(context, "gl_accounts");
        AssertTable<JournalEntry>(context, "journal_entries");
        AssertTable<JournalEntryLine>(context, "journal_entry_lines");
        AssertTable<ArInvoice>(context, "ar_invoices");
        AssertTable<OutboxMessage>(context, "outbox_messages");
        AssertTable<InboxMessage>(context, "inbox_messages");
    }

    [Fact]
    public void Model_ConfiguresTenantUniqueInvoiceAndSalesOrderIndex()
    {
        using var context = CreateContext(TenantA);

        var invoiceIndex = context.Model
            .FindEntityType(typeof(ArInvoice))!
            .GetIndexes()
            .Single(index => index.GetDatabaseName() == "uq_ar_invoices_tenant_id_sales_order_id");

        invoiceIndex.IsUnique.Should().BeTrue();
        invoiceIndex.Properties.Select(property => property.Name)
            .Should().Equal(nameof(ArInvoice.TenantId), nameof(ArInvoice.SalesOrderId));

        var accountIndex = context.Model
            .FindEntityType(typeof(GlAccount))!
            .GetIndexes()
            .Single(index => index.GetDatabaseName() == "uq_gl_accounts_tenant_id_account_code");

        accountIndex.IsUnique.Should().BeTrue();
        accountIndex.Properties.Select(property => property.Name)
            .Should().Equal(nameof(GlAccount.TenantId), nameof(GlAccount.AccountCode));
    }

    [Fact]
    public void Model_AppliesTenantQueryFilters()
    {
        using var context = CreateContext(TenantA);

        context.Model.FindEntityType(typeof(GlAccount))!.GetQueryFilter().Should().NotBeNull();
        context.Model.FindEntityType(typeof(JournalEntry))!.GetQueryFilter().Should().NotBeNull();
        context.Model.FindEntityType(typeof(ArInvoice))!.GetQueryFilter().Should().NotBeNull();
    }

    [Fact]
    public async Task Query_WhenTenantsShareDatabase_ReturnsOnlyCurrentTenantRows()
    {
        var options = CreateOptions($"omnimanage-finance-filter-{Guid.NewGuid():N}");
        var invoiceAId = Guid.NewGuid();
        var invoiceBId = Guid.NewGuid();

        await SeedInvoiceAsync(options, TenantA, invoiceAId, "AR-A-001");
        await SeedInvoiceAsync(options, TenantB, invoiceBId, "AR-B-001");

        await using var contextA = new FinanceDbContext(options, new TestTenantContext(TenantA));
        var invoicesA = await contextA.ArInvoices.ToListAsync();

        invoicesA.Should().ContainSingle();
        invoicesA[0].Id.Should().Be(invoiceAId);
        invoicesA[0].TenantId.Should().Be(TenantA);

        await using var contextB = new FinanceDbContext(options, new TestTenantContext(TenantB));
        var invoicesB = await contextB.ArInvoices.ToListAsync();

        invoicesB.Should().ContainSingle();
        invoicesB[0].Id.Should().Be(invoiceBId);
        invoicesB[0].TenantId.Should().Be(TenantB);
    }

    [Fact]
    public async Task SaveChanges_WhenAggregateBelongsToAnotherTenant_ThrowsTenantIsolationException()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase($"omnimanage-finance-stamp-{Guid.NewGuid():N}")
            .AddTenantIsolation(new TestTenantContext(TenantA))
            .Options;

        await using var context = new FinanceDbContext(options, new TestTenantContext(TenantA));

        context.ArInvoices.Add(CreateInvoice(TenantB, Guid.NewGuid(), "AR-B-001"));

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<TenantIsolationException>();
    }

    [Fact]
    public async Task SaveChanges_WhenJournalEntryIsModified_ThrowsBecausePostedEntriesAreImmutable()
    {
        await using var context = CreateContext(TenantA);
        var entry = CreateJournalEntry(TenantA);
        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        context.Entry(entry).State = EntityState.Modified;

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    [Fact]
    public async Task SaveChanges_WhenJournalEntryLineIsModified_ThrowsBecausePostedEntriesAreImmutable()
    {
        await using var context = CreateContext(TenantA);
        var entry = CreateJournalEntry(TenantA);
        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        context.Entry(entry.Lines[0]).State = EntityState.Modified;

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    [Fact]
    public async Task SaveChanges_WhenJournalEntryIsDeleted_ThrowsBecausePostedEntriesAreImmutable()
    {
        await using var context = CreateContext(TenantA);
        var entry = CreateJournalEntry(TenantA);
        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        context.JournalEntries.Remove(entry);

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    [Fact]
    public async Task SaveChanges_PersistsInvoiceAndJournalTableWithBalancedLines()
    {
        var options = CreateOptions($"omnimanage-finance-persist-{Guid.NewGuid():N}");
        var invoiceId = Guid.NewGuid();
        var entryId = Guid.NewGuid();

        await using (var context = new FinanceDbContext(options, new TestTenantContext(TenantA)))
        {
            context.ArInvoices.Add(CreateInvoice(TenantA, invoiceId, "AR-A-100"));
            context.JournalEntries.Add(CreateJournalEntry(TenantA, entryId));
            await context.SaveChangesAsync();
        }

        await using var reloaded = new FinanceDbContext(options, new TestTenantContext(TenantA));

        var invoice = await reloaded.ArInvoices.SingleAsync();
        invoice.Id.Should().Be(invoiceId);
        invoice.TotalAmount.Should().Be(100m);
        invoice.Status.Should().Be(ArInvoiceStatus.Unpaid);

        var entry = await reloaded.JournalEntries
            .Include(journalEntry => journalEntry.Lines)
            .SingleAsync();
        entry.Id.Should().Be(entryId);
        entry.Lines.Should().HaveCount(2);
        entry.DebitTotal.Should().Be(100m);
        entry.CreditTotal.Should().Be(100m);
    }

    private static void AssertTable<TEntity>(FinanceDbContext context, string expectedTable)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        entityType.Should().NotBeNull();
        entityType!.GetSchema().Should().Be(FinanceDbContext.Schema);
        entityType!.GetTableName().Should().Be(expectedTable);
    }

    private static FinanceDbContext CreateContext(Guid tenantId) =>
        new(
            CreateOptions($"omnimanage-finance-{Guid.NewGuid():N}"),
            new TestTenantContext(tenantId));

    private static DbContextOptions<FinanceDbContext> CreateOptions(string databaseName) =>
        new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

    private static async Task SeedInvoiceAsync(
        DbContextOptions<FinanceDbContext> options,
        Guid tenantId,
        Guid invoiceId,
        string invoiceNumber)
    {
        await using var context = new FinanceDbContext(options, new TestTenantContext(tenantId));
        context.ArInvoices.Add(CreateInvoice(tenantId, invoiceId, invoiceNumber));
        await context.SaveChangesAsync();
    }

    private static ArInvoice CreateInvoice(Guid tenantId, Guid invoiceId, string invoiceNumber) =>
        ArInvoice.Create(
            invoiceId,
            tenantId,
            invoiceNumber,
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            Now,
            UserId).Value;

    private static JournalEntry CreateJournalEntry(Guid tenantId) =>
        CreateJournalEntry(tenantId, Guid.NewGuid());

    private static JournalEntry CreateJournalEntry(Guid tenantId, Guid entryId) =>
        JournalEntry.Create(
            entryId,
            tenantId,
            $"JE-{entryId:N}",
            Now,
            "A/R invoice posting",
            [
                new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 100m, 0m),
                new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 100m),
            ],
            Now,
            UserId).Value;
}
