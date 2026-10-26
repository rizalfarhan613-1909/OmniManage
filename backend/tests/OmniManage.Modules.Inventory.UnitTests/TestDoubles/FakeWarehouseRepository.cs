using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Warehouses;

namespace OmniManage.Modules.Inventory.UnitTests.TestDoubles;

internal sealed class FakeWarehouseRepository : IWarehouseRepository
{
    public List<Warehouse> Warehouses { get; } = [];

    public bool CodeExists { get; set; }

    public WarehouseListCriteria? LastCriteria { get; private set; }

    public int SaveCount { get; private set; }

    public Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        Warehouses.Add(warehouse);
        return Task.CompletedTask;
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(CodeExists);

    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Warehouses.SingleOrDefault(warehouse => warehouse.Id == id));

    public Task<(IReadOnlyList<Warehouse> Warehouses, long TotalItems)> ListAsync(
        WarehouseListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;

        IReadOnlyList<Warehouse> warehouses = Warehouses
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToArray();

        return Task.FromResult((warehouses, (long)Warehouses.Count));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
