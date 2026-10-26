using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.Domain.Orders.Events;

namespace OmniManage.Modules.Sales.Domain.Orders;

public sealed class SalesOrder : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int OrderNumberMaxLength = 50;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;

    private readonly List<SalesOrderLine> _lines = [];

    private SalesOrder()
    {
    }

    private SalesOrder(
        Guid id,
        Guid tenantId,
        string orderNumber,
        Guid customerId,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Status = SalesOrderStatus.Draft;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string OrderNumber { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public SalesOrderStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }

    public IReadOnlyList<SalesOrderLine> Lines => _lines;

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<SalesOrder> Create(
        Guid id,
        Guid tenantId,
        string? orderNumber,
        Guid customerId,
        IReadOnlyList<SalesOrderLineInput>? lines,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return SalesOrderErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return SalesOrderErrors.InvalidTenantId;
        }

        if (!TryNormalizeOrderNumber(orderNumber, out var normalizedOrderNumber))
        {
            return SalesOrderErrors.InvalidOrderNumber;
        }

        if (customerId == Guid.Empty)
        {
            return SalesOrderErrors.InvalidCustomerId;
        }

        if (lines is null || lines.Count == 0)
        {
            return SalesOrderErrors.NoLines;
        }

        if (createdAt == default)
        {
            return SalesOrderErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return SalesOrderErrors.InvalidCreatedBy;
        }

        var order = new SalesOrder(id, tenantId, normalizedOrderNumber, customerId, createdAt, createdBy);
        var totalAmount = 0m;

        foreach (var lineInput in lines)
        {
            var line = SalesOrderLine.Create(
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
                return SalesOrderErrors.InvalidTotalAmount;
            }
        }

        if (totalAmount > MaxAmount)
        {
            return SalesOrderErrors.InvalidTotalAmount;
        }

        order.TotalAmount = totalAmount;

        return order;
    }

    public Result Confirm(Guid eventId, DateTimeOffset occurredAtUtc, Guid updatedBy)
    {
        if (Status != SalesOrderStatus.Draft)
        {
            return SalesOrderErrors.InvalidState;
        }

        if (_lines.Count == 0)
        {
            return SalesOrderErrors.NoLines;
        }

        if (eventId == Guid.Empty)
        {
            return SalesOrderErrors.InvalidEventId;
        }

        if (occurredAtUtc == default)
        {
            return SalesOrderErrors.InvalidUpdatedAt;
        }

        if (updatedBy == Guid.Empty)
        {
            return SalesOrderErrors.InvalidUpdatedBy;
        }

        Status = SalesOrderStatus.Confirmed;
        UpdatedAt = occurredAtUtc;
        UpdatedBy = updatedBy;

        Raise(new SalesOrderConfirmedDomainEvent(eventId, Id, TenantId, occurredAtUtc));

        return Result.Success();
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
}
