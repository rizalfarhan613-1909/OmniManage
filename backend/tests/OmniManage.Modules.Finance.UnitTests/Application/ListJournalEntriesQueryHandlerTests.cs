using FluentAssertions;
using OmniManage.Modules.Finance.Application.Journal.ListJournalEntries;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Errors;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.UnitTests.TestDoubles;

namespace OmniManage.Modules.Finance.UnitTests.Application;

public sealed class ListJournalEntriesQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000951");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000952");
    private static readonly Guid ReceivableAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000953");
    private static readonly Guid RevenueAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000954");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithDateRangeAndSort_PassesCriteriaAndReturnsDtos()
    {
        var journalEntries = new FakeJournalEntryRepository();
        journalEntries.JournalEntries.Add(CreateJournalEntry("JE-1001", 10m));

        var handler = new ListJournalEntriesQueryHandler(journalEntries);
        var from = Now.AddDays(-1);
        var to = Now.AddDays(1);

        var result = await handler.Handle(
            new ListJournalEntriesQuery(
                Page: 1,
                PageSize: 10,
                Search: "  je-100  ",
                PostedFromUtc: from,
                PostedToUtc: to,
                Sort: "-entryNumber"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var page = result.Value;
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(10);
        page.TotalItems.Should().Be(1);
        page.Items.Should().ContainSingle();

        var dto = page.Items[0];
        dto.EntryNumber.Should().Be("JE-1001");
        dto.DebitTotal.Should().Be("10.0000");
        dto.CreditTotal.Should().Be("10.0000");
        dto.Lines.Should().HaveCount(2);
        dto.Lines.Should().ContainSingle(line =>
            line.GlAccountId == ReceivableAccountId
            && line.Debit == "10.0000"
            && line.Credit == "0.0000");
        dto.Lines.Should().ContainSingle(line =>
            line.GlAccountId == RevenueAccountId
            && line.Debit == "0.0000"
            && line.Credit == "10.0000");

        var criteria = journalEntries.LastCriteria;
        criteria.Should().NotBeNull();
        criteria!.Skip.Should().Be(0);
        criteria.Take.Should().Be(10);
        criteria.Search.Should().Be("je-100");
        criteria.PostedFromUtc.Should().Be(from);
        criteria.PostedToUtc.Should().Be(to);
        criteria.Sort.Should().ContainSingle()
            .Which.Should().Be(new JournalEntrySortOption(JournalEntrySortField.EntryNumber, Descending: true));
    }

    [Fact]
    public async Task Handle_WithInvertedDateRange_ReturnsValidationErrorWithoutQuerying()
    {
        var journalEntries = new FakeJournalEntryRepository();
        var handler = new ListJournalEntriesQueryHandler(journalEntries);

        var result = await handler.Handle(
            new ListJournalEntriesQuery(
                PostedFromUtc: Now,
                PostedToUtc: Now.AddDays(-1)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JournalEntryErrors.InvalidDateRange);
        journalEntries.LastCriteria.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithoutSort_UsesPostedDateAscendingByDefault()
    {
        var journalEntries = new FakeJournalEntryRepository();
        var handler = new ListJournalEntriesQueryHandler(journalEntries);

        var result = await handler.Handle(new ListJournalEntriesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        journalEntries.LastCriteria!.Sort.Should().ContainSingle()
            .Which.Should().Be(new JournalEntrySortOption(JournalEntrySortField.PostedDateUtc, Descending: false));
    }

    private static JournalEntry CreateJournalEntry(string entryNumber, decimal amount) =>
        JournalEntry.Create(
            Guid.NewGuid(),
            TenantId,
            entryNumber,
            Now,
            "A/R invoice posting",
            [
                new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, amount, 0m),
                new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, amount),
            ],
            Now,
            UserId).Value;
}
