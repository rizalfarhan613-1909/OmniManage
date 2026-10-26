using FluentAssertions;
using OmniManage.Modules.Inventory.Application.Warehouses.ListWarehouses;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.UnitTests.TestDoubles;

namespace OmniManage.Modules.Inventory.UnitTests.Application;

public sealed class ListWarehousesQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000741");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000742");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_ClampsPaginationAndParsesSort()
    {
        var repository = new FakeWarehouseRepository();
        repository.Warehouses.Add(CreateWarehouse(1, "WH-001", "Main One"));
        repository.Warehouses.Add(CreateWarehouse(2, "WH-002", "Main Two"));

        var handler = new ListWarehousesQueryHandler(repository);

        var result = await handler.Handle(
            new ListWarehousesQuery(Page: 0, PageSize: 500, Search: "  main  ", IsActive: true, Sort: "-name"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(100);
        result.Value.TotalItems.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);

        repository.LastCriteria.Should().NotBeNull();
        repository.LastCriteria!.Skip.Should().Be(0);
        repository.LastCriteria.Take.Should().Be(100);
        repository.LastCriteria.Search.Should().Be("main");
        repository.LastCriteria.IsActive.Should().BeTrue();
        repository.LastCriteria.Sort.Should().ContainSingle();
        repository.LastCriteria.Sort[0].Field.Should().Be(WarehouseSortField.Name);
        repository.LastCriteria.Sort[0].Descending.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithUnknownSortField_FallsBackToCodeAscending()
    {
        var repository = new FakeWarehouseRepository();
        var handler = new ListWarehousesQueryHandler(repository);

        var result = await handler.Handle(
            new ListWarehousesQuery(Page: 2, PageSize: 10, Sort: "address"),
            CancellationToken.None);

        result.Value.Page.Should().Be(2);
        repository.LastCriteria!.Skip.Should().Be(10);
        repository.LastCriteria.Sort[0].Field.Should().Be(WarehouseSortField.Code);
        repository.LastCriteria.Sort[0].Descending.Should().BeFalse();
    }

    private static Warehouse CreateWarehouse(int sequence, string code, string name) =>
        Warehouse.Create(
            Guid.Parse($"0192f5b4-8a00-7000-8000-{sequence:D12}"),
            TenantId,
            code,
            name,
            null,
            Now,
            UserId).Value;
}
