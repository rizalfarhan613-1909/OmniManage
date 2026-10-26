using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Domain.Errors;
using OmniManage.Modules.Inventory.Domain.Warehouses;

namespace OmniManage.Modules.Inventory.UnitTests.Domain;

public sealed class WarehouseTests
{
    private static readonly Guid Id = Guid.Parse("0192f5b4-8a00-7000-8000-000000000701");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000702");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000703");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_NormalizesAndInitializesAggregate()
    {
        var result = Create(
            code: "  wh-main  ",
            name: "  Main Warehouse  ",
            address: "  1 Industrial Way  ");

        result.IsSuccess.Should().BeTrue();

        var warehouse = result.Value;
        warehouse.Id.Should().Be(Id);
        warehouse.TenantId.Should().Be(TenantId);
        warehouse.Code.Should().Be("WH-MAIN");
        warehouse.Name.Should().Be("Main Warehouse");
        warehouse.Address.Should().Be("1 Industrial Way");
        warehouse.IsActive.Should().BeTrue();
        warehouse.CreatedAt.Should().Be(Now);
        warehouse.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public void Create_WhenAddressIsBlank_StoresNull()
    {
        var result = Create(address: "   ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Address.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    [InlineData(" WH 1")]
    [InlineData("WH/1")]
    public void Create_WhenCodeIsInvalid_ReturnsInvalidCode(string? code)
    {
        var result = Create(code: code);

        result.Error.Should().Be(WarehouseErrors.InvalidCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsMissing_ReturnsInvalidName(string? name)
    {
        var result = Create(name: name);

        result.Error.Should().Be(WarehouseErrors.InvalidName);
    }

    [Fact]
    public void Create_WhenNameExceedsMaximumLength_ReturnsInvalidName()
    {
        var result = Create(name: new string('N', Warehouse.NameMaxLength + 1));

        result.Error.Should().Be(WarehouseErrors.InvalidName);
    }

    [Fact]
    public void Create_WhenAddressExceedsMaximumLength_ReturnsInvalidAddress()
    {
        var result = Create(address: new string('A', Warehouse.AddressMaxLength + 1));

        result.Error.Should().Be(WarehouseErrors.InvalidAddress);
    }

    [Fact]
    public void Create_WhenTenantIsEmpty_ReturnsInvalidTenantId()
    {
        var result = Warehouse.Create(Id, Guid.Empty, "WH-1", "Warehouse", null, Now, UserId);

        result.Error.Should().Be(WarehouseErrors.InvalidTenantId);
    }

    [Fact]
    public void Create_WhenIdentifierIsEmpty_ReturnsInvalidId()
    {
        var result = Warehouse.Create(Guid.Empty, TenantId, "WH-1", "Warehouse", null, Now, UserId);

        result.Error.Should().Be(WarehouseErrors.InvalidId);
    }

    [Fact]
    public void Deactivate_UpdatesLifecycleFields()
    {
        var warehouse = Create().Value;

        var result = warehouse.Deactivate(Now.AddDays(1), UserId);

        result.IsSuccess.Should().BeTrue();
        warehouse.IsActive.Should().BeFalse();
        warehouse.UpdatedAt.Should().Be(Now.AddDays(1));
        warehouse.UpdatedBy.Should().Be(UserId);
    }

    [Fact]
    public void Deactivate_WhenUserIsMissing_ReturnsInvalidUpdatedBy()
    {
        var warehouse = Create().Value;

        var result = warehouse.Deactivate(Now.AddDays(1), Guid.Empty);

        result.Error.Should().Be(WarehouseErrors.InvalidUpdatedBy);
        warehouse.IsActive.Should().BeTrue();
    }

    private static Result<Warehouse> Create(
        string? code = "WH-1",
        string? name = "Warehouse One",
        string? address = null) =>
        Warehouse.Create(Id, TenantId, code, name, address, Now, UserId);
}
