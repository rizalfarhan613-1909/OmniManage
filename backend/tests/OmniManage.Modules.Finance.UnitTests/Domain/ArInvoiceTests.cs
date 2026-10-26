using FluentAssertions;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Errors;

namespace OmniManage.Modules.Finance.UnitTests.Domain;

public sealed class ArInvoiceTests
{
    private static readonly Guid InvoiceId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000921");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000922");
    private static readonly Guid SalesOrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000923");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000924");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000925");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_StartsUnpaid()
    {
        var result = ArInvoice.Create(
            InvoiceId,
            TenantId,
            "  ar-001  ",
            SalesOrderId,
            CustomerId,
            99.95m,
            Now,
            UserId);

        result.IsSuccess.Should().BeTrue();

        var invoice = result.Value;
        invoice.Id.Should().Be(InvoiceId);
        invoice.TenantId.Should().Be(TenantId);
        invoice.InvoiceNumber.Should().Be("AR-001");
        invoice.SalesOrderId.Should().Be(SalesOrderId);
        invoice.CustomerId.Should().Be(CustomerId);
        invoice.TotalAmount.Should().Be(99.95m);
        invoice.Status.Should().Be(ArInvoiceStatus.Unpaid);
        invoice.CreatedAt.Should().Be(Now);
        invoice.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public void BuildInvoiceNumber_FormatsTheSalesOrderIdentifier()
    {
        ArInvoice.BuildInvoiceNumber(SalesOrderId)
            .Should().Be($"AR-{SalesOrderId:D}".ToUpperInvariant());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(1_000_000_000_000_000)]
    public void Create_WithInvalidTotalAmount_ReturnsValidationError(decimal totalAmount)
    {
        var result = ArInvoice.Create(
            InvoiceId,
            TenantId,
            "AR-001",
            SalesOrderId,
            CustomerId,
            totalAmount,
            Now,
            UserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ArInvoiceErrors.InvalidTotalAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-AR-001")]
    [InlineData("AR/001")]
    public void Create_WithInvalidInvoiceNumber_ReturnsValidationError(string invoiceNumber)
    {
        var result = ArInvoice.Create(
            InvoiceId,
            TenantId,
            invoiceNumber,
            SalesOrderId,
            CustomerId,
            10m,
            Now,
            UserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ArInvoiceErrors.InvalidInvoiceNumber);
    }

    [Fact]
    public void Create_WithEmptyIdentifiers_ReturnsValidationErrors()
    {
        ArInvoice.Create(
            Guid.Empty,
            TenantId,
            "AR-001",
            SalesOrderId,
            CustomerId,
            10m,
            Now,
            UserId).Error.Should().Be(ArInvoiceErrors.InvalidId);

        ArInvoice.Create(
            InvoiceId,
            Guid.Empty,
            "AR-001",
            SalesOrderId,
            CustomerId,
            10m,
            Now,
            UserId).Error.Should().Be(ArInvoiceErrors.InvalidTenantId);

        ArInvoice.Create(
            InvoiceId,
            TenantId,
            "AR-001",
            Guid.Empty,
            CustomerId,
            10m,
            Now,
            UserId).Error.Should().Be(ArInvoiceErrors.InvalidSalesOrderId);

        ArInvoice.Create(
            InvoiceId,
            TenantId,
            "AR-001",
            SalesOrderId,
            Guid.Empty,
            10m,
            Now,
            UserId).Error.Should().Be(ArInvoiceErrors.InvalidCustomerId);

        ArInvoice.Create(
            InvoiceId,
            TenantId,
            "AR-001",
            SalesOrderId,
            CustomerId,
            10m,
            default,
            UserId).Error.Should().Be(ArInvoiceErrors.InvalidCreatedAt);

        ArInvoice.Create(
            InvoiceId,
            TenantId,
            "AR-001",
            SalesOrderId,
            CustomerId,
            10m,
            Now,
            Guid.Empty).Error.Should().Be(ArInvoiceErrors.InvalidCreatedBy);
    }
}
