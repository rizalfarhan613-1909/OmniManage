using FluentAssertions;
using OmniManage.Modules.Finance.Domain.Errors;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.UnitTests.Domain;

public sealed class JournalEntryTests
{
    private static readonly Guid EntryId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000911");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000912");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000913");
    private static readonly Guid ReceivableAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000914");
    private static readonly Guid RevenueAccountId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000915");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithBalancedLines_ComputesTotalsAndKeepsLines()
    {
        var result = CreateEntry(
            new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 125.5m, 0m),
            new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 125.5m));

        result.IsSuccess.Should().BeTrue();

        var entry = result.Value;
        entry.Id.Should().Be(EntryId);
        entry.TenantId.Should().Be(TenantId);
        entry.EntryNumber.Should().Be("JE-001");
        entry.Description.Should().Be("A/R invoice AR-001");
        entry.PostedDateUtc.Should().Be(Now);
        entry.CreatedAt.Should().Be(Now);
        entry.CreatedBy.Should().Be(UserId);
        entry.Lines.Should().HaveCount(2);
        entry.DebitTotal.Should().Be(125.5m);
        entry.CreditTotal.Should().Be(125.5m);
        entry.Lines.Should().ContainSingle(line =>
            line.GlAccountId == ReceivableAccountId
            && line.Debit == 125.5m
            && line.Credit == 0m);
        entry.Lines.Should().ContainSingle(line =>
            line.GlAccountId == RevenueAccountId
            && line.Debit == 0m
            && line.Credit == 125.5m);
    }

    [Fact]
    public void Create_WithUnbalancedLines_ReturnsUnbalancedError()
    {
        var result = CreateEntry(
            new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 100m, 0m),
            new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 99m));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JournalEntryErrors.Unbalanced);
    }

    [Fact]
    public void Create_WithoutLines_ReturnsNoLinesError()
    {
        var result = JournalEntry.Create(
            EntryId,
            TenantId,
            "JE-001",
            Now,
            "A/R invoice AR-001",
            null,
            Now,
            UserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JournalEntryErrors.NoLines);
    }

    [Fact]
    public void Create_WithSingleLine_ReturnsNoLinesError()
    {
        var result = CreateEntry(
            new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 100m, 0m));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JournalEntryErrors.NoLines);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10, 10)]
    public void Create_WithInvalidLineAmounts_ReturnsInvalidLineAmount(decimal debit, decimal credit)
    {
        var result = CreateEntry(
            new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, debit, credit),
            new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 10m));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JournalEntryErrors.InvalidLineAmount);
    }

    [Fact]
    public void Create_WithEmptyLineIdentifiers_ReturnsValidationErrors()
    {
        CreateEntry(
            new JournalEntryLineInput(Guid.Empty, ReceivableAccountId, 10m, 0m),
            new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 10m))
            .Error.Should().Be(JournalEntryErrors.InvalidLineId);

        CreateEntry(
            new JournalEntryLineInput(Guid.NewGuid(), Guid.Empty, 10m, 0m),
            new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 10m))
            .Error.Should().Be(JournalEntryErrors.InvalidGlAccountId);
    }

    [Fact]
    public void Create_WhenTotalsExceedSupportedAmount_ReturnsInvalidLineAmount()
    {
        var result = CreateEntry(
            new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 1_000_000_000_000_000m, 0m),
            new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 1_000_000_000_000_000m));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JournalEntryErrors.InvalidLineAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-JE-001")]
    public void Create_WithInvalidEntryNumber_ReturnsValidationError(string entryNumber)
    {
        var result = JournalEntry.Create(
            EntryId,
            TenantId,
            entryNumber,
            Now,
            "A/R invoice AR-001",
            [
                new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 10m, 0m),
                new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 10m),
            ],
            Now,
            UserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(JournalEntryErrors.InvalidEntryNumber.Code);
    }

    [Fact]
    public void Create_WithEmptyIdentifiers_ReturnsValidationErrors()
    {
        JournalEntry.Create(
            Guid.Empty,
            TenantId,
            "JE-001",
            Now,
            "A/R invoice AR-001",
            [
                new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 10m, 0m),
                new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 10m),
            ],
            Now,
            UserId).Error.Should().Be(JournalEntryErrors.InvalidId);

        JournalEntry.Create(
            EntryId,
            Guid.Empty,
            "JE-001",
            Now,
            "A/R invoice AR-001",
            [
                new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 10m, 0m),
                new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 10m),
            ],
            Now,
            UserId).Error.Should().Be(JournalEntryErrors.InvalidTenantId);

        JournalEntry.Create(
            EntryId,
            TenantId,
            "JE-001",
            default,
            "A/R invoice AR-001",
            [
                new JournalEntryLineInput(Guid.NewGuid(), ReceivableAccountId, 10m, 0m),
                new JournalEntryLineInput(Guid.NewGuid(), RevenueAccountId, 0m, 10m),
            ],
            Now,
            UserId).Error.Should().Be(JournalEntryErrors.InvalidPostedDate);
    }

    private static OmniManage.BuildingBlocks.Domain.Result<JournalEntry> CreateEntry(
        params JournalEntryLineInput[] lines) =>
        JournalEntry.Create(
            EntryId,
            TenantId,
            "JE-001",
            Now,
            "A/R invoice AR-001",
            lines,
            Now,
            UserId);
}
