using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Domain.Errors;
using OmniManage.Modules.MasterData.Domain.Items;

namespace OmniManage.Modules.MasterData.UnitTests.Domain;

public sealed class ItemTests
{
    private static readonly Guid Id = Guid.Parse("0192f5b4-8a00-7000-8000-000000000611");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000612");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000613");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_NormalizesAndInitializesAggregate()
    {
        var result = Item.Create(
            Id,
            TenantId,
            "  sku-001  ",
            "  Widget  ",
            " pcs ",
            "  Hardware  ",
            25.75m,
            18.25m,
            isStockTracked: true,
            Now,
            UserId);

        result.IsSuccess.Should().BeTrue();
        var item = result.Value;
        item.Id.Should().Be(Id);
        item.TenantId.Should().Be(TenantId);
        item.Sku.Should().Be("SKU-001");
        item.Name.Should().Be("Widget");
        item.UomCode.Should().Be("PCS");
        item.Category.Should().Be("Hardware");
        item.BasePrice.Should().Be(25.75m);
        item.CostPrice.Should().Be(18.25m);
        item.IsStockTracked.Should().BeTrue();
        item.IsActive.Should().BeTrue();
        item.CreatedAt.Should().Be(Now);
        item.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public void Create_WhenCategoryIsBlank_StoresNull()
    {
        var result = Create(category: "   ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    [InlineData(" SKU 1")]
    [InlineData("SKU/1")]
    public void Create_WhenSkuIsInvalid_ReturnsInvalidSku(string? sku)
    {
        var result = Create(sku: sku);

        result.Error.Should().Be(ItemErrors.InvalidSku);
    }

    [Fact]
    public void Create_WhenSkuExceedsMaximumLength_ReturnsInvalidSku()
    {
        var result = Create(sku: new string('A', Item.SkuMaxLength + 1));

        result.Error.Should().Be(ItemErrors.InvalidSku);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsMissing_ReturnsInvalidName(string? name)
    {
        var result = Create(name: name);

        result.Error.Should().Be(ItemErrors.InvalidName);
    }

    [Fact]
    public void Create_WhenNameExceedsMaximumLength_ReturnsInvalidName()
    {
        var result = Create(name: new string('N', Item.NameMaxLength + 1));

        result.Error.Should().Be(ItemErrors.InvalidName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("P")]
    [InlineData("P C")]
    [InlineData("PCS-1")]
    public void Create_WhenUomCodeIsInvalid_ReturnsInvalidUomCode(string? uomCode)
    {
        var result = Create(uomCode: uomCode);

        result.Error.Should().Be(ItemErrors.InvalidUomCode);
    }

    [Fact]
    public void Create_WhenCategoryExceedsMaximumLength_ReturnsInvalidCategory()
    {
        var result = Create(category: new string('C', Item.CategoryMaxLength + 1));

        result.Error.Should().Be(ItemErrors.InvalidCategory);
    }

    [Fact]
    public void Create_WhenBasePriceIsNegative_ReturnsInvalidBasePrice()
    {
        var result = Create(basePrice: -0.01m);

        result.Error.Should().Be(ItemErrors.InvalidBasePrice);
    }

    [Fact]
    public void Create_WhenCostPriceExceedsMaximum_ReturnsInvalidCostPrice()
    {
        var result = Create(costPrice: Item.PriceMax + 1m);

        result.Error.Should().Be(ItemErrors.InvalidCostPrice);
    }

    [Fact]
    public void Create_WhenTenantIsEmpty_ReturnsInvalidTenantId()
    {
        var result = Item.Create(
            Id,
            Guid.Empty,
            "SKU-001",
            "Widget",
            "PCS",
            null,
            10m,
            5m,
            true,
            Now,
            UserId);

        result.Error.Should().Be(ItemErrors.InvalidTenantId);
    }

    [Fact]
    public void Create_WhenIdentifierIsEmpty_ReturnsInvalidId()
    {
        var result = Item.Create(
            Guid.Empty,
            TenantId,
            "SKU-001",
            "Widget",
            "PCS",
            null,
            10m,
            5m,
            true,
            Now,
            UserId);

        result.Error.Should().Be(ItemErrors.InvalidId);
    }

    private static Result<Item> Create(
        string? sku = "SKU-001",
        string? name = "Widget",
        string? uomCode = "PCS",
        string? category = null,
        decimal basePrice = 10m,
        decimal costPrice = 5m) =>
        Item.Create(Id, TenantId, sku, name, uomCode, category, basePrice, costPrice, true, Now, UserId);
}
