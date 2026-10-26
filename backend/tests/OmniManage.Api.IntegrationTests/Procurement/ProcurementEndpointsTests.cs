using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Procurement.Application.Common;
using OmniManage.Modules.Procurement.Application.PurchaseOrders;
using OmniManage.Modules.Procurement.Application.Suppliers;

namespace OmniManage.Api.IntegrationTests.Procurement;

public sealed class ProcurementEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string SuppliersPath = "/api/v1/procurement/suppliers";
    private const string PurchaseOrdersPath = "/api/v1/procurement/purchase-orders";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreatePurchaseOrder_WithValidRequest_ReturnsCreatedDraft()
    {
        using var client = await CreateAuthorizedClientAsync();
        var orderNumber = UniqueOrderNumber("PO");

        var response = await CreatePurchaseOrderAsync(
            client,
            $" {orderNumber.ToLowerInvariant()} ",
            ApiFactory.AcmeSupplierId,
            [(Guid.NewGuid(), 2m, 10.25m), (Guid.NewGuid(), 1m, 2.5m)]);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PurchaseOrderDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Error.Should().BeNull();
        payload.Data.Should().NotBeNull();

        var purchaseOrder = payload.Data!;
        purchaseOrder.OrderNumber.Should().Be(orderNumber);
        purchaseOrder.SupplierId.Should().Be(ApiFactory.AcmeSupplierId);
        purchaseOrder.SupplierName.Should().Be("Acme Industrial Supplies");
        purchaseOrder.Status.Should().Be("Draft");
        purchaseOrder.TotalAmount.Should().Be("23.0000");
        purchaseOrder.Lines.Should().HaveCount(2);
        purchaseOrder.Lines.Select(line => line.Quantity)
            .Should().BeEquivalentTo(["2.0000", "1.0000"]);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Be($"{PurchaseOrdersPath}/{purchaseOrder.Id:D}");
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithDuplicateOrderNumber_ReturnsConflict()
    {
        using var client = await CreateAuthorizedClientAsync();
        var orderNumber = UniqueOrderNumber("DUP");

        (await CreatePurchaseOrderAsync(
                client,
                orderNumber,
                ApiFactory.AcmeSupplierId,
                [(Guid.NewGuid(), 1m, 1m)]))
            .EnsureSuccessStatusCode();

        var response = await CreatePurchaseOrderAsync(
            client,
            orderNumber.ToLowerInvariant(),
            ApiFactory.AcmeSupplierId,
            [(Guid.NewGuid(), 1m, 1m)]);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PROCUREMENT.PURCHASE_ORDER.ORDER_NUMBER_ALREADY_EXISTS");
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithUnknownSupplier_ReturnsNotFound()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreatePurchaseOrderAsync(
            client,
            UniqueOrderNumber("MISS"),
            Guid.NewGuid(),
            [(Guid.NewGuid(), 1m, 1m)]);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PROCUREMENT.PURCHASE_ORDER.SUPPLIER_NOT_FOUND");
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithoutLines_ReturnsValidationError()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreatePurchaseOrderAsync(
            client,
            UniqueOrderNumber("NOLINE"),
            ApiFactory.AcmeSupplierId,
            []);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PROCUREMENT.PURCHASE_ORDER.NO_LINES");
    }

    [Fact]
    public async Task ListPurchaseOrders_WithSearchSortAndPagination_ReturnsPagedEnvelope()
    {
        using var client = await CreateAuthorizedClientAsync();
        var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        foreach (var suffix in new[] { "A", "B", "C" })
        {
            (await CreatePurchaseOrderAsync(
                    client,
                    $"PO-{token}-{suffix}",
                    ApiFactory.AcmeSupplierId,
                    [(Guid.NewGuid(), 1m, 1m)]))
                .EnsureSuccessStatusCode();
        }

        var response = await client.GetAsync(
            new Uri(
                $"{PurchaseOrdersPath}?search=po-{token}&sort=-orderNumber&page=1&pageSize=2",
                UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<PurchaseOrderDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var page = payload.Data!;
        page.TotalItems.Should().Be(3);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalPages.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.Items.Select(purchaseOrder => purchaseOrder.OrderNumber)
            .Should().ContainInOrder($"PO-{token}-C", $"PO-{token}-B");

        payload.Meta.Pagination.Should().NotBeNull();
        payload.Meta.Pagination!.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task ListSuppliers_WithSearch_ReturnsSeededSupplier()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(
            new Uri($"{SuppliersPath}?search={ApiFactory.AcmeSupplierCode}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<SupplierDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var supplier = payload.Data!.Items.Should().ContainSingle().Subject;
        supplier.Id.Should().Be(ApiFactory.AcmeSupplierId);
        supplier.Code.Should().Be(ApiFactory.AcmeSupplierCode);
        supplier.Name.Should().Be("Acme Industrial Supplies");
        supplier.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ListSuppliers_ExcludesRecordsFromOtherTenants()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(
            new Uri($"{SuppliersPath}?search={ApiFactory.BetaSupplierCode}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<SupplierDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.TotalItems.Should().Be(0);
        payload.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ListSuppliers_WithLimitedUser_ReturnsForbidden()
    {
        using var client = await CreateAuthorizedClientAsync(ApiFactory.LimitedEmail);

        var response = await client.GetAsync(new Uri(SuppliersPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.FORBIDDEN");
    }

    [Fact]
    public async Task ListPurchaseOrders_WhenModuleNotLicensed_ReturnsModuleNotLicensed()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var response = await client.GetAsync(new Uri(PurchaseOrdersPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PLATFORM.MODULE_NOT_LICENSED");
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

    private static Task<HttpResponseMessage> CreatePurchaseOrderAsync(
        HttpClient client,
        string orderNumber,
        Guid supplierId,
        IReadOnlyList<(Guid ItemId, decimal Quantity, decimal UnitPrice)> lines) =>
        client.PostAsJsonAsync(
            PurchaseOrdersPath,
            new
            {
                orderNumber,
                supplierId,
                expectedDeliveryDate = (DateOnly?)null,
                notes = "Integration test purchase order",
                lines = lines.Select(line => new
                {
                    itemId = line.ItemId,
                    quantity = line.Quantity,
                    unitPrice = line.UnitPrice,
                }),
            },
            JsonOptions);

    private static string UniqueOrderNumber(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
}
