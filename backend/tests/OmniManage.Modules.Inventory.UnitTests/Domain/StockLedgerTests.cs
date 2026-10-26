using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Domain.Errors;
using OmniManage.Modules.Inventory.Domain.Stock;

namespace OmniManage.Modules.Inventory.UnitTests.Domain;

public sealed class StockLedgerTests
{
    private static readonly Guid Id = Guid.Parse("0192f5b4-8a00-7000-8000-000000000711");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000712");
    private static readonly Guid WarehouseId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000713");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000714");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_InitializesImmutableEntry()
    {
        var result = Create(movementType: " receive ", referenceNumber: "  GRN-001  ");

        result.IsSuccess.Should().BeTrue();

        var entry = result.Value;
        entry.Id.Should().Be(Id);
        entry.TenantId.Should().Be(TenantId);
        entry.WarehouseId.Should().Be(WarehouseId);
        entry.ItemId.Should().Be(ItemId);
        entry.QuantityDelta.Should().Be(12.5m);
        entry.MovementType.Should().Be(MovementType.Receive);
        entry.ReferenceNumber.Should().Be("GRN-001");
        entry.OccurredAtUtc.Should().Be(Now);
    }

    [Theory]
    [InlineData("receive", MovementType.Receive)]
    [InlineData("ISSUE", MovementType.Issue)]
    [InlineData("Transfer", MovementType.Transfer)]
    [InlineData("adjustment", MovementType.Adjustment)]
    public void Create_ParsesSupportedMovementTypes(string value, MovementType expected)
    {
        var result = Create(movementType: value);

        result.IsSuccess.Should().BeTrue();
        result.Value.MovementType.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("4")]
    [InlineData("Unknown")]
    public void Create_WhenMovementTypeIsInvalid_ReturnsInvalidMovementType(string? movementType)
    {
        var result = Create(movementType: movementType);

        result.Error.Should().Be(StockErrors.InvalidMovementType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1_000_000_000_000_000)]
    public void Create_WhenQuantityDeltaIsInvalid_ReturnsInvalidQuantityDelta(decimal quantityDelta)
    {
        var result = Create(quantityDelta: quantityDelta);

        result.Error.Should().Be(StockErrors.InvalidQuantityDelta);
    }

    [Fact]
    public void Create_WhenReferenceNumberIsMissing_ReturnsInvalidReferenceNumber()
    {
        var result = Create(referenceNumber: "   ");

        result.Error.Should().Be(StockErrors.InvalidReferenceNumber);
    }

    [Fact]
    public void Create_WhenReferenceNumberExceedsMaximumLength_ReturnsInvalidReferenceNumber()
    {
        var result = Create(referenceNumber: new string('R', StockLedger.ReferenceNumberMaxLength + 1));

        result.Error.Should().Be(StockErrors.InvalidReferenceNumber);
    }

    [Fact]
    public void Create_WhenWarehouseIsEmpty_ReturnsInvalidWarehouseId()
    {
        var result = StockLedger.Create(Id, TenantId, Guid.Empty, ItemId, 1m, "Receive", "REF-1", Now);

        result.Error.Should().Be(StockErrors.InvalidWarehouseId);
    }

    [Fact]
    public void Create_WhenItemIsEmpty_ReturnsInvalidItemId()
    {
        var result = StockLedger.Create(Id, TenantId, WarehouseId, Guid.Empty, 1m, "Receive", "REF-1", Now);

        result.Error.Should().Be(StockErrors.InvalidItemId);
    }

    [Fact]
    public void Create_WhenOccurredAtIsMissing_ReturnsInvalidOccurredAtUtc()
    {
        var result = StockLedger.Create(Id, TenantId, WarehouseId, ItemId, 1m, "Receive", "REF-1", default);

        result.Error.Should().Be(StockErrors.InvalidOccurredAtUtc);
    }

    private static Result<StockLedger> Create(
        decimal quantityDelta = 12.5m,
        string? movementType = "Receive",
        string? referenceNumber = "GRN-001") =>
        StockLedger.Create(
            Id,
            TenantId,
            WarehouseId,
            ItemId,
            quantityDelta,
            movementType,
            referenceNumber,
            Now);
}
