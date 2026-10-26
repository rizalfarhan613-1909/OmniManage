using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Inventory.Application.Common;
using OmniManage.Modules.Inventory.Application.Stock;
using OmniManage.Modules.Inventory.Application.Warehouses;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Api.IntegrationTests.Inventory;

public sealed class InventoryEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string WarehousesPath = "/api/v1/inventory/warehouses";
    private const string StockBalancesPath = "/api/v1/inventory/stock/balances";
    private const string StockAdjustmentsPath = "/api/v1/inventory/stock/adjustments";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateWarehouse_WithValidRequest_ReturnsCreated()
    {
        using var client = await CreateAuthorizedClientAsync();
        var code = UniqueCode("WH");

        var response = await CreateWarehouseAsync(client, $" {code.ToLowerInvariant()} ", "Main Warehouse", "1 Industrial Way");

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<WarehouseDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Error.Should().BeNull();
        payload.Data.Should().NotBeNull();

        var warehouse = payload.Data!;
        warehouse.Code.Should().Be(code);
        warehouse.Name.Should().Be("Main Warehouse");
        warehouse.Address.Should().Be("1 Industrial Way");
        warehouse.IsActive.Should().BeTrue();
        warehouse.CreatedAt.Should().NotBe(default);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"{WarehousesPath}/{warehouse.Id:D}");
    }

    [Fact]
    public async Task CreateWarehouse_WithDuplicateNormalizedCode_ReturnsConflict()
    {
        using var client = await CreateAuthorizedClientAsync();
        var code = UniqueCode("DUP");

        (await CreateWarehouseAsync(client, code, "First Warehouse")).EnsureSuccessStatusCode();

        var response = await CreateWarehouseAsync(client, code.ToLowerInvariant(), "Second Warehouse");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("INVENTORY.WAREHOUSE.CODE_ALREADY_EXISTS");
    }

    [Fact]
    public async Task ListWarehouses_WithSearchSortAndPagination_ReturnsPagedEnvelope()
    {
        using var client = await CreateAuthorizedClientAsync();
        var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        (await CreateWarehouseAsync(client, $"PGN{token}-A", "Pagination Alpha")).EnsureSuccessStatusCode();
        (await CreateWarehouseAsync(client, $"PGN{token}-B", "Pagination Bravo")).EnsureSuccessStatusCode();
        (await CreateWarehouseAsync(client, $"PGN{token}-C", "Pagination Charlie")).EnsureSuccessStatusCode();

        var response = await client.GetAsync(
            new Uri($"{WarehousesPath}?search=pgn{token}&sort=-code&page=1&pageSize=2", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<WarehouseDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var page = payload.Data!;
        page.TotalItems.Should().Be(3);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalPages.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.Items.Select(warehouse => warehouse.Code)
            .Should().ContainInOrder($"PGN{token}-C", $"PGN{token}-B");

        payload.Meta.Pagination.Should().NotBeNull();
        payload.Meta.Pagination!.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task AdjustStock_ThenGetBalances_AggregatesLedgerByItemAndWarehouse()
    {
        using var client = await CreateAuthorizedClientAsync();
        var warehouse = await CreateWarehouseOrThrowAsync(client, UniqueCode("BAL"));
        var itemOne = Guid.NewGuid();
        var itemTwo = Guid.NewGuid();
        var reference = UniqueCode("REF");

        (await AdjustStockAsync(client, warehouse.Id, itemOne, 10m, "Receive", $"{reference}-1"))
            .EnsureSuccessStatusCode();
        (await AdjustStockAsync(client, warehouse.Id, itemOne, -3m, "Issue", $"{reference}-2"))
            .EnsureSuccessStatusCode();
        (await AdjustStockAsync(client, warehouse.Id, itemOne, 1.5m, "Adjustment", $"{reference}-3"))
            .EnsureSuccessStatusCode();
        (await AdjustStockAsync(client, warehouse.Id, itemTwo, 2m, "Receive", $"{reference}-4"))
            .EnsureSuccessStatusCode();

        var response = await client.GetAsync(
            new Uri($"{StockBalancesPath}?warehouseId={warehouse.Id:D}&page=1&pageSize=10", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<StockBalanceDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var balances = payload.Data!;
        balances.TotalItems.Should().Be(2);
        balances.Items.Should().HaveCount(2);

        var itemOneBalance = balances.Items.Single(balance => balance.ItemId == itemOne);
        itemOneBalance.WarehouseId.Should().Be(warehouse.Id);
        itemOneBalance.QuantityOnHand.Should().Be("8.5000");

        var itemTwoBalance = balances.Items.Single(balance => balance.ItemId == itemTwo);
        itemTwoBalance.QuantityOnHand.Should().Be("2.0000");

        var filtered = await client.GetAsync(
            new Uri(
                $"{StockBalancesPath}?warehouseId={warehouse.Id:D}&itemId={itemOne:D}",
                UriKind.Relative));

        filtered.StatusCode.Should().Be(HttpStatusCode.OK);

        var filteredPayload = await filtered.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<StockBalanceDto>>>(JsonOptions);

        filteredPayload.Should().NotBeNull();
        filteredPayload!.Data.Should().NotBeNull();
        filteredPayload.Data!.TotalItems.Should().Be(1);
        filteredPayload.Data.Items.Should().ContainSingle()
            .Which.QuantityOnHand.Should().Be("8.5000");
    }

    [Fact]
    public async Task AdjustStock_WithZeroQuantity_ReturnsValidationError()
    {
        using var client = await CreateAuthorizedClientAsync();
        var warehouse = await CreateWarehouseOrThrowAsync(client, UniqueCode("ZERO"));

        var response = await AdjustStockAsync(
            client,
            warehouse.Id,
            Guid.NewGuid(),
            0m,
            "Adjustment",
            UniqueCode("ADJ"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("INVENTORY.STOCK.INVALID_QUANTITY_DELTA");
    }

    [Fact]
    public async Task AdjustStock_WithUnknownWarehouse_ReturnsNotFound()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await AdjustStockAsync(
            client,
            Guid.NewGuid(),
            Guid.NewGuid(),
            1m,
            "Receive",
            UniqueCode("MISS"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("INVENTORY.WAREHOUSE.NOT_FOUND");
    }

    [Fact]
    public async Task ListWarehouses_ExcludesRecordsFromOtherTenants()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(
            new Uri($"{WarehousesPath}?search={ApiFactory.BetaWarehouseCode}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<WarehouseDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.TotalItems.Should().Be(0);
        payload.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ListWarehouses_WithoutToken_ReturnsUnauthenticated()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri(WarehousesPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.UNAUTHENTICATED");
    }

    [Fact]
    public async Task ListWarehouses_WithLimitedUser_ReturnsForbidden()
    {
        using var client = await CreateAuthorizedClientAsync(ApiFactory.LimitedEmail);

        var response = await client.GetAsync(new Uri(WarehousesPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.FORBIDDEN");
    }

    [Fact]
    public async Task ListWarehouses_WhenModuleNotLicensed_ReturnsModuleNotLicensed()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var response = await client.GetAsync(new Uri(WarehousesPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PLATFORM.MODULE_NOT_LICENSED");
        payload.Error.Details.Should().ContainSingle()
            .Which.Field.Should().Be("moduleCode");
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(
        string email = ApiFactory.Email,
        string tenantCode = ApiFactory.TenantCode)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                tenantCode,
                email,
                password = ApiFactory.Password,
            },
            JsonOptions);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", payload.Data!.AccessToken);

        return client;
    }

    private static Task<HttpResponseMessage> CreateWarehouseAsync(
        HttpClient client,
        string code,
        string name,
        string? address = null) =>
        client.PostAsJsonAsync(WarehousesPath, new { code, name, address }, JsonOptions);

    private static async Task<WarehouseDto> CreateWarehouseOrThrowAsync(HttpClient client, string code)
    {
        var response = await CreateWarehouseAsync(client, code, $"Warehouse {code}");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<WarehouseDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();

        return payload.Data!;
    }

    private static Task<HttpResponseMessage> AdjustStockAsync(
        HttpClient client,
        Guid warehouseId,
        Guid itemId,
        decimal quantityDelta,
        string movementType,
        string referenceNumber) =>
        client.PostAsJsonAsync(
            StockAdjustmentsPath,
            new { warehouseId, itemId, quantityDelta, movementType, referenceNumber },
            JsonOptions);

    private static string UniqueCode(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
}
