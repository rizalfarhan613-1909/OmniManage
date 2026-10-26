using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Domain.Errors;

namespace OmniManage.Modules.Manufacturing.Domain.WorkOrders;

public sealed class WorkOrder : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int OrderNumberMaxLength = 50;
    public const int BillOfMaterialCodeMaxLength = 50;
    public const int NotesMaxLength = 1000;
    public const decimal MaxQuantity = 999_999_999.999999m;

    private WorkOrder()
    {
    }

    private WorkOrder(
        Guid id,
        Guid tenantId,
        string orderNumber,
        Guid productItemId,
        Guid billOfMaterialId,
        string billOfMaterialCode,
        decimal quantityPlanned,
        DateOnly? dueDate,
        string? notes,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        OrderNumber = orderNumber;
        ProductItemId = productItemId;
        BillOfMaterialId = billOfMaterialId;
        BillOfMaterialCode = billOfMaterialCode;
        Status = WorkOrderStatus.Draft;
        QuantityPlanned = quantityPlanned;
        QuantityProduced = 0m;
        DueDate = dueDate;
        Notes = notes;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string OrderNumber { get; private set; } = string.Empty;

    public Guid ProductItemId { get; private set; }

    public Guid BillOfMaterialId { get; private set; }

    /// <summary>
    /// Snapshot of the BOM code at release time. Work orders are execution
    /// documents, so later BOM revisions must not rewrite their history.
    /// </summary>
    public string BillOfMaterialCode { get; private set; } = string.Empty;

    public WorkOrderStatus Status { get; private set; }

    public decimal QuantityPlanned { get; private set; }

    public decimal QuantityProduced { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<WorkOrder> Create(
        Guid id,
        Guid tenantId,
        string? orderNumber,
        Guid productItemId,
        Guid billOfMaterialId,
        string? billOfMaterialCode,
        decimal quantityPlanned,
        DateOnly? dueDate,
        string? notes,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return WorkOrderErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return WorkOrderErrors.InvalidTenantId;
        }

        if (!TryNormalizeOrderNumber(orderNumber, out var normalizedOrderNumber))
        {
            return WorkOrderErrors.InvalidOrderNumber;
        }

        if (productItemId == Guid.Empty)
        {
            return WorkOrderErrors.InvalidProductItemId;
        }

        if (billOfMaterialId == Guid.Empty)
        {
            return WorkOrderErrors.InvalidBillOfMaterialId;
        }

        if (!TryNormalizeBillOfMaterialCode(billOfMaterialCode, out var normalizedBillOfMaterialCode))
        {
            return WorkOrderErrors.InvalidBillOfMaterialCode;
        }

        if (quantityPlanned is <= 0m or > MaxQuantity)
        {
            return WorkOrderErrors.InvalidQuantityPlanned;
        }

        if (!TryNormalizeNotes(notes, out var normalizedNotes))
        {
            return WorkOrderErrors.InvalidNotes;
        }

        if (createdAt == default)
        {
            return WorkOrderErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return WorkOrderErrors.InvalidCreatedBy;
        }

        if (dueDate is { } scheduledDate
            && scheduledDate < DateOnly.FromDateTime(createdAt.UtcDateTime))
        {
            return WorkOrderErrors.InvalidDueDate;
        }

        return new WorkOrder(
            id,
            tenantId,
            normalizedOrderNumber,
            productItemId,
            billOfMaterialId,
            normalizedBillOfMaterialCode,
            quantityPlanned,
            dueDate,
            normalizedNotes,
            createdAt,
            createdBy);
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

    private static bool TryNormalizeBillOfMaterialCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length > BillOfMaterialCodeMaxLength)
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
