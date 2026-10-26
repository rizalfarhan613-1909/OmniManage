using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Domain.Errors;

namespace OmniManage.Modules.Procurement.Domain.PurchaseOrders;

public sealed class PurchaseOrder : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int OrderNumberMaxLength = 50;
    public const int SupplierNameMaxLength = 200;
    public const int NotesMaxLength = 1000;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;

    private readonly List<PurchaseOrderLine> _lines = [];

    private PurchaseOrder()
    {
    }

    private PurchaseOrder(
        Guid id,
        Guid tenantId,
        string orderNumber,
        Guid supplierId,
        string supplierName,
        DateOnly? expectedDeliveryDate,
        string? notes,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        OrderNumber = orderNumber;
        SupplierId = supplierId;
        SupplierName = supplierName;
        Status = PurchaseOrderStatus.Draft;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Notes = notes;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string OrderNumber { get; private set; } = string.Empty;

    public Guid SupplierId { get; private set; }

    /// <summary>
    /// Snapshot of the supplier name at order time. Purchase orders are
    /// documents, so later vendor renames must not rewrite history.
    /// </summary>
    public string SupplierName { get; private set; } = string.Empty;

    public PurchaseOrderStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }

    public DateOnly? ExpectedDeliveryDate { get; private set; }

    public string? Notes { get; private set; }

    public IReadOnlyList<PurchaseOrderLine> Lines => _lines;

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<PurchaseOrder> Create(
        Guid id,
        Guid tenantId,
        string? orderNumber,
        Guid supplierId,
        string? supplierName,
        DateOnly? expectedDeliveryDate,
        string? notes,
        IReadOnlyList<PurchaseOrderLineInput>? lines,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return PurchaseOrderErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return PurchaseOrderErrors.InvalidTenantId;
        }

        if (!TryNormalizeOrderNumber(orderNumber, out var normalizedOrderNumber))
        {
            return PurchaseOrderErrors.InvalidOrderNumber;
        }

        if (supplierId == Guid.Empty)
        {
            return PurchaseOrderErrors.InvalidSupplierId;
        }

        if (!TryNormalizeSupplierName(supplierName, out var normalizedSupplierName))
        {
            return PurchaseOrderErrors.InvalidSupplierName;
        }

        if (!TryNormalizeNotes(notes, out var normalizedNotes))
        {
            return PurchaseOrderErrors.InvalidNotes;
        }

        if (createdAt == default)
        {
            return PurchaseOrderErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return PurchaseOrderErrors.InvalidCreatedBy;
        }

        if (expectedDeliveryDate is { } deliveryDate
            && deliveryDate < DateOnly.FromDateTime(createdAt.UtcDateTime))
        {
            return PurchaseOrderErrors.InvalidExpectedDeliveryDate;
        }

        if (lines is null || lines.Count == 0)
        {
            return PurchaseOrderErrors.NoLines;
        }

        var order = new PurchaseOrder(
            id,
            tenantId,
            normalizedOrderNumber,
            supplierId,
            normalizedSupplierName,
            expectedDeliveryDate,
            normalizedNotes,
            createdAt,
            createdBy);

        var totalAmount = 0m;

        foreach (var lineInput in lines)
        {
            var line = PurchaseOrderLine.Create(
                lineInput.Id,
                order.Id,
                lineInput.ItemId,
                lineInput.Quantity,
                lineInput.UnitPrice);

            if (line.IsFailure)
            {
                return line.Error;
            }

            order._lines.Add(line.Value);

            try
            {
                totalAmount = decimal.Round(
                    totalAmount + line.Value.LineTotal,
                    4,
                    MidpointRounding.AwayFromZero);
            }
            catch (OverflowException)
            {
                return PurchaseOrderErrors.InvalidTotalAmount;
            }
        }

        if (totalAmount > MaxAmount)
        {
            return PurchaseOrderErrors.InvalidTotalAmount;
        }

        order.TotalAmount = totalAmount;

        return order;
    }

    private static bool TryNormalizeOrderNumber(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length > OrderNumberMaxLength)
        {
            return false;
        }

        if (!char.IsAsciiLetterOrDigit(candidate[0]))
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

    private static bool TryNormalizeSupplierName(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > SupplierNameMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeNotes(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > NotesMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
