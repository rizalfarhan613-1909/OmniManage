using FluentAssertions;
using OmniManage.Modules.Inventory.Application.Warehouses.CreateWarehouse;
using OmniManage.Modules.Inventory.Domain.Errors;
using OmniManage.Modules.Inventory.UnitTests.TestDoubles;

namespace OmniManage.Modules.Inventory.UnitTests.Application;

public sealed class CreateWarehouseCommandHandlerTests
{
    private static readonly Guid WarehouseId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000721");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000722");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000723");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAndReturnsDto()
    {
        var repository = new FakeWarehouseRepository();
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateWarehouseCommand(" wh-main ", "Main Warehouse", "1 Industrial Way"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("WH-MAIN");
        result.Value.Name.Should().Be("Main Warehouse");
        result.Value.Address.Should().Be("1 Industrial Way");
        result.Value.IsActive.Should().BeTrue();

        repository.Warehouses.Should().ContainSingle();
        repository.Warehouses[0].TenantId.Should().Be(TenantId);
        repository.Warehouses[0].CreatedBy.Should().Be(UserId);
        repository.Warehouses[0].CreatedAt.Should().Be(Now);
        repository.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_ReturnsConflictWithoutSaving()
    {
        var repository = new FakeWarehouseRepository { CodeExists = true };
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateWarehouseCommand("WH-MAIN", "Main Warehouse", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WarehouseErrors.CodeAlreadyExists);
        repository.Warehouses.Should().BeEmpty();
        repository.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenCodeIsInvalid_ReturnsDomainValidationError()
    {
        var repository = new FakeWarehouseRepository();
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateWarehouseCommand(" ", "Main Warehouse", null),
            CancellationToken.None);

        result.Error.Should().Be(WarehouseErrors.InvalidCode);
        repository.SaveCount.Should().Be(0);
    }

    private static CreateWarehouseCommandHandler CreateHandler(FakeWarehouseRepository repository) =>
        new(
            repository,
            new TestTenantContext(TenantId),
            new FakeCurrentUser(UserId),
            new FixedGuidGenerator(WarehouseId),
            new FixedTimeProvider(Now));
}
