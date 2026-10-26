using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.Modules.Finance.Application.Common;
using OmniManage.Modules.Finance.Application.IntegrationEventHandlers;
using OmniManage.Modules.Finance.Application.Invoices;
using OmniManage.Modules.Finance.Application.Journal;
using OmniManage.Modules.Finance.Contracts.IntegrationEvents;
using OmniManage.Modules.Finance.Domain.Gl;
using OmniManage.Modules.Finance.Infrastructure.Persistence;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Sales.Application.Orders;

namespace OmniManage.Api.IntegrationTests.Finance;

public sealed class FinanceEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string InvoicesPath = "/api/v1/finance/invoices";
    private const string JournalEntriesPath = "/api/v1/finance/journal-entries";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ConfirmOrder_CreatesArInvoiceAndBalancedJournalAndConsumesEvent()
    {
        using var client = await CreateAuthorizedClientAsync();
        var customerId = Guid.NewGuid();
        var order = await CreateOrderOrThrowAsync(
            client,
            UniqueOrderNumber("FIN"),
            customerId,
            [(Guid.NewGuid(), 2m, 12.5m), (Guid.NewGuid(), 1m, 5m)]);

        var confirmResponse = await ConfirmOrderAsync(client, order.Id);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoice = await GetSingleInvoiceAsync(client, order.Id);
        invoice.InvoiceNumber.Should().Be($"AR-{order.Id:D}".ToUpperInvariant());
        invoice.SalesOrderId.Should().Be(order.Id);
        invoice.CustomerId.Should().Be(customerId);
        invoice.TotalAmount.Should().Be("30.0000");
        invoice.Status.Should().Be("Unpaid");

        var journalEntry = await GetSingleJournalEntryAsync(client, order.Id);
        journalEntry.EntryNumber.Should().Be($"JE-{order.Id:D}".ToUpperInvariant());
        journalEntry.DebitTotal.Should().Be("30.0000");
        journalEntry.CreditTotal.Should().Be("30.0000");
        journalEntry.Lines.Should().HaveCount(2);

        using var scope = factory.Services.CreateScope();
        var finance = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

        var accounts = await finance.GlAccounts
            .IgnoreQueryFilters()
            .Where(account => account.TenantId == ApiFactory.TenantId)
            .ToListAsync();

        var receivable = accounts.Single(account =>
            account.AccountCode == ChartOfAccounts.AccountsReceivableCode);
        var revenue = accounts.Single(account =>
            account.AccountCode == ChartOfAccounts.SalesRevenueCode);

        journalEntry.Lines.Should().ContainSingle(line =>
            line.GlAccountId == receivable.Id
            && line.Debit == "30.0000"
            && line.Credit == "0.0000");
        journalEntry.Lines.Should().ContainSingle(line =>
            line.GlAccountId == revenue.Id
            && line.Debit == "0.0000"
            && line.Credit == "30.0000");

        var inboxMessages = await finance.InboxMessages
            .Where(message =>
                message.TenantId == ApiFactory.TenantId
                && message.Status == InboxStatus.Processed
                && message.Consumer.Contains(nameof(SalesOrderConfirmedIntegrationEventHandler)))
            .ToListAsync();

        inboxMessages.Should().NotBeEmpty();

        var outboxMessages = await finance.OutboxMessages.ToListAsync();
        var postedEvent = outboxMessages.SingleOrDefault(message =>
            message.Type.Contains(nameof(ArInvoicePostedIntegrationEvent), StringComparison.Ordinal)
            && message.Payload.Contains(order.Id.ToString(), StringComparison.OrdinalIgnoreCase));

        postedEvent.Should().NotBeNull();
        postedEvent!.TenantId.Should().Be(ApiFactory.TenantId);
    }

    [Fact]
    public async Task ListInvoices_WithStatusAndPagination_ReturnsPagedUnpaidInvoices()
    {
        using var client = await CreateAuthorizedClientAsync();

        foreach (var amount in new[] { 10m, 20m, 30m })
        {
            var order = await CreateOrderOrThrowAsync(
                client,
                UniqueOrderNumber("PAGE"),
                Guid.NewGuid(),
                [(Guid.NewGuid(), 1m, amount)]);
            (await ConfirmOrderAsync(client, order.Id)).EnsureSuccessStatusCode();
        }

        var response = await client.GetAsync(
            new Uri(
                $"{InvoicesPath}?page=1&pageSize=2&status=Unpaid&sort=-createdAt",
                UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<ArInvoiceDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        var page = payload.Data!;
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.Items.Should().HaveCount(2);
        page.TotalItems.Should().BeGreaterThanOrEqualTo(3);
        page.Items.Should().OnlyContain(invoice => invoice.Status == "Unpaid");
    }

    [Fact]
    public async Task ListInvoices_ExcludesRecordsFromOtherTenants()
    {
        using var client = await CreateAuthorizedClientAsync();

        var invoicesResponse = await client.GetAsync(
            new Uri($"{InvoicesPath}?search={ApiFactory.BetaArInvoiceNumber}", UriKind.Relative));

        invoicesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoicesPayload = await invoicesResponse.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<ArInvoiceDto>>>(JsonOptions);

        invoicesPayload.Should().NotBeNull();
        invoicesPayload!.Data.Should().NotBeNull();
        invoicesPayload.Data!.TotalItems.Should().Be(0);
        invoicesPayload.Data.Items.Should().BeEmpty();

        var journalResponse = await client.GetAsync(
            new Uri($"{JournalEntriesPath}?search={ApiFactory.BetaJournalEntryNumber}", UriKind.Relative));

        journalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var journalPayload = await journalResponse.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<JournalEntryDto>>>(JsonOptions);

        journalPayload.Should().NotBeNull();
        journalPayload!.Data.Should().NotBeNull();
        journalPayload.Data!.TotalItems.Should().Be(0);
        journalPayload.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ListInvoices_WithoutToken_ReturnsUnauthenticated()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri(InvoicesPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.UNAUTHENTICATED");
    }

    [Fact]
    public async Task ListInvoices_WithLimitedUser_ReturnsForbidden()
    {
        using var client = await CreateAuthorizedClientAsync(ApiFactory.LimitedEmail);

        var response = await client.GetAsync(new Uri(InvoicesPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.FORBIDDEN");
    }

    [Fact]
    public async Task ListInvoices_WhenModuleNotLicensed_ReturnsModuleNotLicensed()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var response = await client.GetAsync(new Uri(InvoicesPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PLATFORM.MODULE_NOT_LICENSED");
        payload.Error.Details.Should().ContainSingle()
            .Which.Field.Should().Be("moduleCode");
        payload.Error.Details.Should().ContainSingle()
            .Which.Message.Should().Be(ModuleCodes.FinAr);
    }

    [Fact]
    public async Task ListJournalEntries_WhenModuleNotLicensed_ReturnsModuleNotLicensed()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApiFactory.BetaEmail,
            ApiFactory.BetaTenantCode);

        var response = await client.GetAsync(new Uri(JournalEntriesPath, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PLATFORM.MODULE_NOT_LICENSED");
        payload.Error.Details.Should().ContainSingle()
            .Which.Message.Should().Be(ModuleCodes.FinGl);
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

    private static async Task<ArInvoiceDto> GetSingleInvoiceAsync(HttpClient client, Guid salesOrderId)
    {
        var response = await client.GetAsync(
            new Uri(
                $"{InvoicesPath}?search={Uri.EscapeDataString(salesOrderId.ToString("D"))}",
                UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<ArInvoiceDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.TotalItems.Should().Be(1);
        payload.Data.Items.Should().ContainSingle();

        return payload.Data.Items[0];
    }

    private static async Task<JournalEntryDto> GetSingleJournalEntryAsync(HttpClient client, Guid salesOrderId)
    {
        var entryNumber = $"JE-{salesOrderId:D}".ToUpperInvariant();

        var response = await client.GetAsync(
            new Uri(
                $"{JournalEntriesPath}?search={Uri.EscapeDataString(entryNumber)}",
                UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<JournalEntryDto>>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.TotalItems.Should().Be(1);
        payload.Data.Items.Should().ContainSingle();

        return payload.Data.Items[0];
    }

    private static Task<HttpResponseMessage> CreateOrderAsync(
        HttpClient client,
        string orderNumber,
        Guid customerId,
        IReadOnlyList<(Guid ItemId, decimal Quantity, decimal UnitPrice)> lines) =>
        client.PostAsJsonAsync(
            "/api/v1/sales/orders",
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
        client.PostAsync(
            new Uri($"/api/v1/sales/orders/{orderId:D}/confirm", UriKind.Relative),
            content: null);

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
