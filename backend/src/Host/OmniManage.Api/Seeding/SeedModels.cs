namespace OmniManage.Api.Seeding;

/// <summary>
/// Canonical seed records shared between the module seeders. They carry just
/// enough information for downstream modules (for example sales need item
/// prices, finance needs confirmed orders) without leaking persistence types.
/// </summary>
internal sealed record MasterDataSeedResult(
    IReadOnlyList<Guid> CustomerIds,
    IReadOnlyList<SeedItem> Items);

internal sealed record SeedItem(
    Guid Id,
    string Sku,
    string Name,
    decimal BasePrice,
    decimal CostPrice,
    bool IsStockTracked);

internal sealed record SeedWarehouse(Guid Id, string Code, string Name);

internal sealed record SeedSalesOrder(
    Guid Id,
    Guid CustomerId,
    string OrderNumber,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    bool IsConfirmed);
