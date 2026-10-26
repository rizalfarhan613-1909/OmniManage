using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.Api.IntegrationTests;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Inventory.Application.Stock;
using OmniManage.Modules.Sales.Application.Common;
using OmniManage.Modules.Sales.Application.Orders;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;
using OmniManage.Modules.Sales.Infrastructure.Persistence;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Api.IntegrationTests.Sales;

public sealed class SalesEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string OrdersPath = "/api/v1/sales/orders";
    private const string StockBalancesPath = "/api/v1/inventory/stock/balances";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateOrder_WithValidRequest_ReturnsCreatedDraft()
    {
        using var client = await CreateAuthorizedClientAsync();
        var orderNumber = UniqueOrderNumber("SO");

        var response = await CreateOrderAsync(
            client,
            $" {orderNumber.ToLowerInvariant()} ",
            Guid.NewGuid(),
            [(Guid.NewGuid(), 2m, 10.25m)]);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SalesOrderDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Error.Should().BeNull();
        payload.Data.Should().NotBeNull();

        var order = payload.Data!;
        order.OrderNumber.Should().Be(orderNumber);
        order.Status.Should().Be("Draft");
        order.TotalAmount.Should().Be("20.5000");
        order.Lines.Should().ContainSingle();
        order.Lines[0].Quantity.Should().Be("2.0000");
        order.Lines[0].UnitPrice.Should().Be("10.2500");

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"{OrdersPath}/{order.Id:D}");
    }

    [Fact]
    public async Task CreateOrder_WithDuplicateOrderNumber_ReturnsConflict()
    {
        using var client = await CreateAuthorizedClientAsync();
        var orderNumber = UniqueOrderNumber("DUP");

        (await CreateOrderAsync(client, orderNumber, Guid.NewGuid(), [(Guid.NewGuid(), 1m, 1m)]))
            .EnsureSuccessStatusCode();

        var response = await CreateOrderAsync(
            client,
            orderNumber.ToLowerInvariant(),
            Guid.NewGuid(),
            [(Guid.NewGuid(), 1m, 1m)]);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("SALES.ORDER.ORDER_NUMBER_ALREADY_EXISTS");
    }

    [Fact]
    public async Task ListOrders_WithSearchSortAndPagination_ReturnsPagedEnvelope()
    {
        using var client = await CreateAuthorizedClientAsync();
        var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        foreach (var suffix in new[] { "A", "B", "C" })
        {
            (await CreateOrderAsync(
                    client,
                    $"LST-{token}-{suffix}",
                    Guid.NewGuid(),
                    [(Guid.NewGuid(), 1m, 1m)]))
                .EnsureSuccessStatusCode();
        }

        var response = await client.GetAsync(
            new Uri($"{OrdersPath}?search=lst-{token}&sort=-orderNumber&page=1&pageSize=2", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<SalesOrderDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var page = payload.Data!;
        page.TotalItems.Should().Be(3);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalPages.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.Items.Select(order => order.OrderNumber)
            .Should().ContainInOrder($"LST-{token}-C", $"LST-{token}-B");
    }

    [Fact]
    public async Task ConfirmOrder_PersistsOutboxAndDeductsStock()
    {
        using var client = await CreateAuthorizedClientAsync();
        var itemId = Guid.NewGuid();
        var order = await CreateOrderOrThrowAsync(
            client,
            UniqueOrderNumber("CNF"),
            Guid.NewGuid(),
            [(itemId, 4m, 12.5m)]);

        var response = await client.PostAsync(
            new Uri($"{OrdersPath}/{order.Id:D}/confirm", UriKind.Relative),
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SalesOrderDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.Status.Should().Be("Confirmed");
        payload.Data.UpdatedAt.Should().NotBeNull();

        var balanceResponse = await client.GetAsync(
            new Uri(
                $"{StockBalancesPath}?warehouseId={ApiFactory.AcmeWarehouseId:D}&itemId={itemId:D}",
                UriKind.Relative));

        balanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var balancePayload = await balanceResponse.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<StockBalanceDto>>>(JsonOptions);

        balancePayload.Should().NotBeNull();
        balancePayload!.Data.Should().NotBeNull();

        var balances = balancePayload.Data!;
        balances.TotalItems.Should().Be(1);

        var balance = balances.Items.Single();
        balance.WarehouseId.Should().Be(ApiFactory.AcmeWarehouseId);
        balance.QuantityOnHand.Should().Be("-4.0000");

        using var scope = factory.Services.CreateScope();
        var salesContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var outboxMessages = await salesContext.OutboxMessages.AsNoTracking().ToListAsync();

        var outboxMessage = outboxMessages.SingleOrDefault(message =>
            message.Type.Contains(nameof(SalesOrderConfirmedIntegrationEvent), StringComparison.Ordinal)
            && message.Payload.Contains(order.Id.ToString(), StringComparison.OrdinalIgnoreCase));

        outboxMessage.Should().NotBeNull();
        outboxMessage!.TenantId.Should().Be(ApiFactory.TenantId);
        outboxMessage.ProcessedOnUtc.Should().NotBeNull();
        outboxMessage.Attempts.Should().Be(0);
        outboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmOrder_WhenAlreadyConfirmed_ReturnsConflict()
    {
        using var client = await CreateAuthorizedClientAsync();
        var order = await CreateOrderOrThrowAsync(
            client,
            UniqueOrderNumber("TWICE"),
            Guid.NewGuid(),
            [(Guid.NewGuid(), 1m, 1m)]);

        (await ConfirmOrderAsync(client, order.Id)).EnsureSuccessStatusCode();

        var response = await ConfirmOrderAsync(client, order.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("SALES.ORDER.INVALID_STATE");
    }

    [Fact]
    public async Task ConfirmOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await ConfirmOrderAsync(client, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("SALES.ORDER.NOT_FOUND");
    }

    [Fact]
    public async Task GetOrder_FromAnotherTenant_ReturnsNotFound()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(
            new Uri($"{OrdersPath}/{ApiFactory.BetaSalesOrderId:D}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("SALES.ORDER.NOT_FOUND");
    }

    [Fact]
    public async Task ListOrders_ExcludesRecordsFromOtherTenants()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(
            new Uri($"{OrdersPath}?search=SO-BETA-0001", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<SalesOrderDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.TotalItems.Should().Be(0);
        payload.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrder_WithLimitedUser_ReturnsForbidden()
    {
        using var client = await CreateAuthorizedClientAsync(ApiFactory.LimitedEmail);

        var response = await CreateOrderAsync(
            client,
            UniqueOrderNumber("LIM"),
            Guid.NewGuid(),
            [(Guid.NewGuid(), 1m, 1m)]);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.FORBIDDEN");
    }

    [Fact]
    public async Task ListOrders_WhenModuleNotLicensed_ReturnsModuleNotLicensed()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var response = await client.GetAsync(new Uri(OrdersPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PLATFORM.MODULE_NOT_LICENSED");
        payload.Error.Details.Should().ContainSingle()
            .Which.Field.Should().Be("moduleCode");
    }

    [Fact]
    public async Task ListOrders_WithoutToken_ReturnsUnauthenticated()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri(OrdersPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.UNAUTHENTICATED");
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

    private static Task<HttpResponseMessage> CreateOrderAsync(
        HttpClient client,
        string orderNumber,
        Guid customerId,
        IReadOnlyList<(Guid ItemId, decimal Quantity, decimal UnitPrice)> lines) =>
        client.PostAsJsonAsync(
            OrdersPath,
            new
            {
                orderNumber,
                customerId,
                lines = lines.Select(line => new
                {
                    itemId = line.ItemId,
                    quantity = line.Quantity,
                    unitPrice = line.UnitPrice,
                }),
            },
            JsonOptions);

    private static Task<HttpResponseMessage> ConfirmOrderAsync(HttpClient client, Guid orderId) =>
        client.PostAsync(new Uri($"{OrdersPath}/{orderId:D}/confirm", UriKind.Relative), content: null);

    private static async Task<SalesOrderDto> CreateOrderOrThrowAsync(
        HttpClient client,
        string orderNumber,
        Guid customerId,
        IReadOnlyList<(Guid ItemId, decimal Quantity, decimal UnitPrice)> lines)
    {
        var response = await CreateOrderAsync(client, orderNumber, customerId, lines);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SalesOrderDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();

        return payload.Data!;
    }

    private static string UniqueOrderNumber(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
}
