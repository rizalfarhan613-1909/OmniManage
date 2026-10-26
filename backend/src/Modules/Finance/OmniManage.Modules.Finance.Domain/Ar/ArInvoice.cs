using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Domain.Errors;

namespace OmniManage.Modules.Finance.Domain.Ar;

public sealed class ArInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int InvoiceNumberMaxLength = 50;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;
    public const string InvoiceNumberPrefix = "AR";

    private ArInvoice()
    {
    }

    private ArInvoice(
        Guid id,
        Guid tenantId,
        string invoiceNumber,
        Guid salesOrderId,
        Guid customerId,
        decimal totalAmount,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        SalesOrderId = salesOrderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Status = ArInvoiceStatus.Unpaid;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string InvoiceNumber { get; private set; } = string.Empty;

    public Guid SalesOrderId { get; private set; }

    public Guid CustomerId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public ArInvoiceStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<ArInvoice> Create(
        Guid id,
        Guid tenantId,
        string? invoiceNumber,
        Guid salesOrderId,
        Guid customerId,
        decimal totalAmount,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return ArInvoiceErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return ArInvoiceErrors.InvalidTenantId;
        }

        if (!TryNormalizeInvoiceNumber(invoiceNumber, out var normalizedInvoiceNumber))
        {
            return ArInvoiceErrors.InvalidInvoiceNumber;
        }

        if (salesOrderId == Guid.Empty)
        {
            return ArInvoiceErrors.InvalidSalesOrderId;
        }

        if (customerId == Guid.Empty)
        {
            return ArInvoiceErrors.InvalidCustomerId;
        }

        if (totalAmount is <= 0m or > MaxAmount)
        {
            return ArInvoiceErrors.InvalidTotalAmount;
        }

        if (createdAt == default)
        {
            return ArInvoiceErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return ArInvoiceErrors.InvalidCreatedBy;
        }

        return new ArInvoice(
            id,
            tenantId,
            normalizedInvoiceNumber,
            salesOrderId,
            customerId,
            totalAmount,
            createdAt,
            createdBy);
    }

    public static string BuildInvoiceNumber(Guid salesOrderId) =>
        $"{InvoiceNumberPrefix}-{salesOrderId:D}".ToUpperInvariant();

    private static bool TryNormalizeInvoiceNumber(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length > InvoiceNumberMaxLength || !char.IsAsciiLetterOrDigit(candidate[0]))
        {
            return false;
        }

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
            {
                return false;
            }
        }

        normalized = candidate;
        return true;
    }
}
