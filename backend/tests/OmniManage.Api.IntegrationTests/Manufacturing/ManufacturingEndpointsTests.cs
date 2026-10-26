using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Manufacturing.Application.BillOfMaterials;
using OmniManage.Modules.Manufacturing.Application.Common;
using OmniManage.Modules.Manufacturing.Application.WorkOrders;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Api.IntegrationTests.Manufacturing;

public sealed class ManufacturingEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string WorkOrdersPath = "/api/v1/manufacturing/work-orders";
    private const string BillOfMaterialsPath = "/api/v1/manufacturing/bom";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateWorkOrder_WithValidRequest_ReturnsCreatedDraft()
    {
        using var client = await CreateAuthorizedClientAsync();
        var orderNumber = UniqueOrderNumber("WO");
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var response = await CreateWorkOrderAsync(
            client,
            $" {orderNumber.ToLowerInvariant()} ",
            ApiFactory.AcmeBomId,
            12.5m,
            dueDate);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<WorkOrderDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Error.Should().BeNull();
        payload.Data.Should().NotBeNull();

        var workOrder = payload.Data!;
        workOrder.OrderNumber.Should().Be(orderNumber);
        workOrder.BillOfMaterialId.Should().Be(ApiFactory.AcmeBomId);
        workOrder.BillOfMaterialCode.Should().Be(ApiFactory.AcmeBomCode);
        workOrder.ProductItemId.Should().Be(ApiFactory.AcmeFinishedGoodItemId);
        workOrder.Status.Should().Be("Draft");
        workOrder.QuantityPlanned.Should().Be("12.5000");
        workOrder.QuantityProduced.Should().Be("0.0000");
        workOrder.DueDate.Should().Be(dueDate);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Be($"{WorkOrdersPath}/{workOrder.Id:D}");
    }

    [Fact]
    public async Task CreateWorkOrder_WithDuplicateOrderNumber_ReturnsConflict()
    {
        using var client = await CreateAuthorizedClientAsync();
        var orderNumber = UniqueOrderNumber("DUP");

        (await CreateWorkOrderAsync(client, orderNumber, ApiFactory.AcmeBomId, 1m, null))
            .EnsureSuccessStatusCode();

        var response = await CreateWorkOrderAsync(
            client,
            orderNumber.ToLowerInvariant(),
            ApiFactory.AcmeBomId,
            1m,
            null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("MANUFACTURING.WORK_ORDER.ORDER_NUMBER_ALREADY_EXISTS");
    }

    [Fact]
    public async Task CreateWorkOrder_WithUnknownBillOfMaterial_ReturnsNotFound()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreateWorkOrderAsync(
            client,
            UniqueOrderNumber("MISS"),
            Guid.NewGuid(),
            1m,
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("MANUFACTURING.WORK_ORDER.BILL_OF_MATERIAL_NOT_FOUND");
    }

    [Fact]
    public async Task CreateWorkOrder_WithZeroQuantity_ReturnsValidationError()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreateWorkOrderAsync(
            client,
            UniqueOrderNumber("ZERO"),
            ApiFactory.AcmeBomId,
            0m,
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("MANUFACTURING.WORK_ORDER.INVALID_QUANTITY_PLANNED");
    }

    [Fact]
    public async Task ListWorkOrders_WithSearchSortAndPagination_ReturnsPagedEnvelope()
    {
        using var client = await CreateAuthorizedClientAsync();
        var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        foreach (var suffix in new[] { "A", "B", "C" })
        {
            (await CreateWorkOrderAsync(
                    client,
                    $"WO-{token}-{suffix}",
                    ApiFactory.AcmeBomId,
                    1m,
                    null))
                .EnsureSuccessStatusCode();
        }

        var response = await client.GetAsync(
            new Uri(
                $"{WorkOrdersPath}?search=wo-{token}&sort=-orderNumber&page=1&pageSize=2",
                UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<WorkOrderDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var page = payload.Data!;
        page.TotalItems.Should().Be(3);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalPages.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.Items.Select(workOrder => workOrder.OrderNumber)
            .Should().ContainInOrder($"WO-{token}-C", $"WO-{token}-B");

        payload.Meta.Pagination.Should().NotBeNull();
        payload.Meta.Pagination!.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task ListBillOfMaterials_WithSearch_ReturnsSeededBom()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(
            new Uri($"{BillOfMaterialsPath}?search={ApiFactory.AcmeBomCode}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<BillOfMaterialDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var billOfMaterial = payload.Data!.Items.Should().ContainSingle().Subject;
        billOfMaterial.Id.Should().Be(ApiFactory.AcmeBomId);
        billOfMaterial.Code.Should().Be(ApiFactory.AcmeBomCode);
        billOfMaterial.ProductItemId.Should().Be(ApiFactory.AcmeFinishedGoodItemId);
        billOfMaterial.Version.Should().Be(1);
        billOfMaterial.IsActive.Should().BeTrue();

        var component = billOfMaterial.Components.Should().ContainSingle().Subject;
        component.ItemId.Should().Be(ApiFactory.AcmeBomComponentItemId);
        component.QuantityPerUnit.Should().Be("2.5000");
    }

    [Fact]
    public async Task ListBillOfMaterials_ExcludesRecordsFromOtherTenants()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync(
            new Uri($"{BillOfMaterialsPath}?search={ApiFactory.BetaBomCode}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<BillOfMaterialDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.TotalItems.Should().Be(0);
        payload.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ListWorkOrders_WithLimitedUser_ReturnsForbidden()
    {
        using var client = await CreateAuthorizedClientAsync(ApiFactory.LimitedEmail);

        var response = await client.GetAsync(new Uri(WorkOrdersPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.FORBIDDEN");
    }

    [Fact]
    public async Task ListWorkOrders_WhenModuleNotLicensed_ReturnsModuleNotLicensed()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var response = await client.GetAsync(new Uri(WorkOrdersPath, UriKind.Relative));

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

    private static Task<HttpResponseMessage> CreateWorkOrderAsync(
        HttpClient client,
        string orderNumber,
        Guid billOfMaterialId,
        decimal quantityPlanned,
        DateOnly? dueDate) =>
        client.PostAsJsonAsync(
            WorkOrdersPath,
            new
            {
                orderNumber,
                billOfMaterialId,
                quantityPlanned,
                dueDate,
                notes = "Integration test work order",
            },
            JsonOptions);

    private static string UniqueOrderNumber(string prefix) =>
        $"{prefix}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
}
