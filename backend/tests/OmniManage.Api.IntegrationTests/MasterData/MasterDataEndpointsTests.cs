using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.MasterData.Application.BusinessPartners;
using OmniManage.Modules.MasterData.Application.Common;
using OmniManage.Modules.MasterData.Application.Items;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Api.IntegrationTests.MasterData;

public sealed class MasterDataEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string BusinessPartnersPath = "/api/v1/masterdata/business-partners";
    private const string ItemsPath = "/api/v1/masterdata/items";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateBusinessPartner_WithValidRequest_ReturnsCreatedAndMaskedTaxId()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreateBusinessPartnerAsync(
            client,
            " acme-cust-01 ",
            "Acme Customer One",
            "vendor",
            "1234567890",
            1500.5m);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BusinessPartnerDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Error.Should().BeNull();
        payload.Data.Should().NotBeNull();

        var businessPartner = payload.Data!;
        businessPartner.Code.Should().Be("ACME-CUST-01");
        businessPartner.Name.Should().Be("Acme Customer One");
        businessPartner.PartnerType.Should().Be("Vendor");
        businessPartner.TaxIdMasked.Should().Be("****7890");
        businessPartner.CreditLimit.Should().Be("1500.5000");
        businessPartner.IsActive.Should().BeTrue();
        businessPartner.CreatedAt.Should().NotBe(default);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Be($"{BusinessPartnersPath}/{businessPartner.Id:D}");
    }

    [Fact]
    public async Task ListBusinessPartners_WithSearchSortAndPagination_ReturnsPagedEnvelope()
    {
        using var client = await CreateAuthorizedClientAsync();

        (await CreateBusinessPartnerAsync(client, "SRT-ALPHA", "Sort Alpha")).EnsureSuccessStatusCode();
        (await CreateBusinessPartnerAsync(client, "SRT-BRAVO", "Sort Bravo")).EnsureSuccessStatusCode();
        (await CreateBusinessPartnerAsync(client, "SRT-CHARLIE", "Sort Charlie")).EnsureSuccessStatusCode();

        var response = await client.GetAsync(
            new Uri($"{BusinessPartnersPath}?search=srt&sort=-code&page=1&pageSize=2", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<BusinessPartnerDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var page = payload.Data!;
        page.TotalItems.Should().Be(3);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalPages.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.Items.Select(businessPartner => businessPartner.Code)
            .Should().ContainInOrder("SRT-CHARLIE", "SRT-BRAVO");

        payload.Meta.Pagination.Should().NotBeNull();
        payload.Meta.Pagination!.Page.Should().Be(1);
        payload.Meta.Pagination.PageSize.Should().Be(2);
        payload.Meta.Pagination.TotalItems.Should().Be(3);
        payload.Meta.Pagination.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task CreateBusinessPartner_WithBlankName_ReturnsValidationError()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreateBusinessPartnerAsync(client, "acme-invalid-01", "   ");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("MASTERDATA.BUSINESS_PARTNER.INVALID_NAME");
    }

    [Fact]
    public async Task CreateBusinessPartner_WithDuplicateNormalizedCode_ReturnsConflict()
    {
        using var client = await CreateAuthorizedClientAsync();

        (await CreateBusinessPartnerAsync(client, "dup-100", "Duplicate One")).EnsureSuccessStatusCode();

        var response = await CreateBusinessPartnerAsync(client, "DUP-100", "Duplicate Two");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("MASTERDATA.BUSINESS_PARTNER.CODE_ALREADY_EXISTS");
    }

    [Fact]
    public async Task CreateItem_WithValidRequest_ReturnsCreated()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreateItemAsync(
            client,
            " sku-100 ",
            "Widget",
            "pcs",
            "Hardware",
            12.5m,
            7.25m,
            isStockTracked: true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ItemDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Error.Should().BeNull();
        payload.Data.Should().NotBeNull();

        var item = payload.Data!;
        item.Sku.Should().Be("SKU-100");
        item.Name.Should().Be("Widget");
        item.UomCode.Should().Be("PCS");
        item.Category.Should().Be("Hardware");
        item.BasePrice.Should().Be("12.5000");
        item.CostPrice.Should().Be("7.2500");
        item.IsStockTracked.Should().BeTrue();
        item.IsActive.Should().BeTrue();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"{ItemsPath}/{item.Id:D}");
    }

    [Fact]
    public async Task ListItems_WithSearch_ReturnsPagedEnvelope()
    {
        using var client = await CreateAuthorizedClientAsync();

        (await CreateItemAsync(client, "ITM-001", "Item One", "pcs", null, 1m, 0.5m, true))
            .EnsureSuccessStatusCode();
        (await CreateItemAsync(client, "ITM-002", "Item Two", "pcs", null, 2m, 1m, true))
            .EnsureSuccessStatusCode();

        var response = await client.GetAsync(
            new Uri($"{ItemsPath}?search=itm&sort=sku&page=1&pageSize=10", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ItemDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var page = payload.Data!;
        page.TotalItems.Should().Be(2);
        page.TotalPages.Should().Be(1);
        page.Items.Select(item => item.Sku).Should().ContainInOrder("ITM-001", "ITM-002");

        payload.Meta.Pagination.Should().NotBeNull();
        payload.Meta.Pagination!.TotalItems.Should().Be(2);
        payload.Meta.Pagination.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task CreateItem_WithNegativeBasePrice_ReturnsValidationError()
    {
        using var client = await CreateAuthorizedClientAsync();

        var response = await CreateItemAsync(client, "ITM-BAD-1", "Invalid Item", "pcs", null, -1m, 0m, true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("MASTERDATA.ITEM.INVALID_BASE_PRICE");
    }

    [Fact]
    public async Task ListBusinessPartners_ExcludesRecordsFromOtherTenants()
    {
        using var client = await CreateAuthorizedClientAsync();

        var partnersResponse = await client.GetAsync(
            new Uri($"{BusinessPartnersPath}?search=beta", UriKind.Relative));
        var itemsResponse = await client.GetAsync(new Uri($"{ItemsPath}?search=beta", UriKind.Relative));

        partnersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        itemsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var partners = await partnersResponse.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<BusinessPartnerDto>>>(JsonOptions);
        var items = await itemsResponse.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<ItemDto>>>(JsonOptions);

        partners.Should().NotBeNull();
        partners!.Data.Should().NotBeNull();
        partners.Data!.TotalItems.Should().Be(0);
        partners.Data.Items.Should().BeEmpty();

        items.Should().NotBeNull();
        items!.Data.Should().NotBeNull();
        items.Data!.TotalItems.Should().Be(0);
        items.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ListBusinessPartners_WithoutToken_ReturnsUnauthenticated()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri(BusinessPartnersPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.UNAUTHENTICATED");
    }

    [Fact]
    public async Task ListBusinessPartners_WithLimitedUser_ReturnsForbidden()
    {
        using var client = await CreateAuthorizedClientAsync(ApiFactory.LimitedEmail);

        var response = await client.GetAsync(new Uri(BusinessPartnersPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.FORBIDDEN");
    }

    [Fact]
    public async Task ListBusinessPartners_WhenModuleNotLicensed_ReturnsModuleNotLicensed()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var response = await client.GetAsync(new Uri(BusinessPartnersPath, UriKind.Relative));

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

    private static Task<HttpResponseMessage> CreateBusinessPartnerAsync(
        HttpClient client,
        string code,
        string name,
        string partnerType = "Customer",
        string? taxId = null,
        decimal creditLimit = 0m) =>
        client.PostAsJsonAsync(
            BusinessPartnersPath,
            new { code, name, partnerType, taxId, creditLimit },
            JsonOptions);

    private static Task<HttpResponseMessage> CreateItemAsync(
        HttpClient client,
        string sku,
        string name,
        string uomCode,
        string? category,
        decimal basePrice,
        decimal costPrice,
        bool isStockTracked) =>
        client.PostAsJsonAsync(
            ItemsPath,
            new { sku, name, uomCode, category, basePrice, costPrice, isStockTracked },
            JsonOptions);
}
