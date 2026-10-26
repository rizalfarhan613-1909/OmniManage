using FluentAssertions;
using OmniManage.Modules.Finance.Application.Invoices.ListArInvoices;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Errors;
using OmniManage.Modules.Finance.UnitTests.TestDoubles;

namespace OmniManage.Modules.Finance.UnitTests.Application;

public sealed class ListArInvoicesQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000941");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000942");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithFiltersAndPagination_NormalizesCriteriaAndReturnsDtos()
    {
        var invoices = new FakeArInvoiceRepository();
        invoices.Invoices.Add(CreateInvoice("AR-1001", 12.5m));
        invoices.Invoices.Add(CreateInvoice("AR-1002", 99m));

        var handler = new ListArInvoicesQueryHandler(invoices);

        var result = await handler.Handle(
            new ListArInvoicesQuery(
                Page: 0,
                PageSize: 500,
                Search: "  ar-100  ",
                Status: "unpaid",
                Sort: "-totalAmount"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var page = result.Value;
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(100);
        page.TotalItems.Should().Be(2);
        page.TotalPages.Should().Be(1);
        page.Items.Should().HaveCount(2);
        page.Items[0].TotalAmount.Should().Be("12.5000");
        page.Items[0].Status.Should().Be("Unpaid");

        var criteria = invoices.LastCriteria;
        criteria.Should().NotBeNull();
        criteria!.Skip.Should().Be(0);
        criteria.Take.Should().Be(100);
        criteria.Search.Should().Be("ar-100");
        criteria.Status.Should().Be(ArInvoiceStatus.Unpaid);
        criteria.Sort.Should().ContainSingle()
            .Which.Should().Be(new ArInvoiceSortOption(ArInvoiceSortField.TotalAmount, Descending: true));
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ReturnsValidationErrorWithoutQuerying()
    {
        var invoices = new FakeArInvoiceRepository();
        var handler = new ListArInvoicesQueryHandler(invoices);

        var result = await handler.Handle(
            new ListArInvoicesQuery(Status: "overdue"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ArInvoiceErrors.InvalidStatusFilter);
        invoices.LastCriteria.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithoutSort_UsesCreatedAtAscendingByDefault()
    {
        var invoices = new FakeArInvoiceRepository();
        var handler = new ListArInvoicesQueryHandler(invoices);

        var result = await handler.Handle(new ListArInvoicesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoices.LastCriteria!.Sort.Should().ContainSingle()
            .Which.Should().Be(new ArInvoiceSortOption(ArInvoiceSortField.CreatedAt, Descending: false));
    }

    private static ArInvoice CreateInvoice(string invoiceNumber, decimal totalAmount) =>
        ArInvoice.Create(
            Guid.NewGuid(),
            TenantId,
            invoiceNumber,
            Guid.NewGuid(),
            Guid.NewGuid(),
            totalAmount,
            Now,
            UserId).Value;
}
