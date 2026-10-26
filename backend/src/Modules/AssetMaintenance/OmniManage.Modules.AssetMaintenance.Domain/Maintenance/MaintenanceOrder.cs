using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.AssetMaintenance.Domain.Errors;

namespace OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

public sealed class MaintenanceOrder : AggregateRoot<Guid>, ITenantEntity
{
    public const int RequestNumberMinLength = 2;
    public const int RequestNumberMaxLength = 50;
    public const int DescriptionMaxLength = 1000;
    public const decimal MaxCost = 999_999_999_999_999.9999m;

    private MaintenanceOrder()
    {
    }

    private MaintenanceOrder(
        Guid id,
        Guid tenantId,
        Guid assetId,
        string requestNumber,
        DateOnly? scheduledDate,
        string description)
        : base(id)
    {
        TenantId = tenantId;
        AssetId = assetId;
        RequestNumber = requestNumber;
        ScheduledDate = scheduledDate;
        Description = description;
        Cost = 0m;
        Status = MaintenanceStatus.Scheduled;
    }

    public Guid TenantId { get; private set; }

    public Guid AssetId { get; private set; }

    public string RequestNumber { get; private set; } = string.Empty;

    public DateOnly? ScheduledDate { get; private set; }

    public DateOnly? CompletedDate { get; private set; }

    public decimal Cost { get; private set; }

    public MaintenanceStatus Status { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public static Result<MaintenanceOrder> Create(
        Guid id,
        Guid tenantId,
        Guid assetId,
        string? requestNumber,
        DateOnly? scheduledDate,
        string? description)
    {
        if (id == Guid.Empty)
        {
            return MaintenanceOrderErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return MaintenanceOrderErrors.InvalidTenantId;
        }

        if (assetId == Guid.Empty)
        {
            return MaintenanceOrderErrors.InvalidAssetId;
        }

        if (!TryNormalizeRequestNumber(requestNumber, out var normalizedRequestNumber))
        {
            return MaintenanceOrderErrors.InvalidRequestNumber;
        }

        if (scheduledDate == default)
        {
            return MaintenanceOrderErrors.InvalidScheduledDate;
        }

        if (!TryNormalizeDescription(description, out var normalizedDescription))
        {
            return MaintenanceOrderErrors.InvalidDescription;
        }

        return new MaintenanceOrder(
            id,
            tenantId,
            assetId,
            normalizedRequestNumber,
            scheduledDate,
            normalizedDescription);
    }

    public Result Start()
    {
        if (Status != MaintenanceStatus.Scheduled)
        {
            return MaintenanceOrderErrors.InvalidStatusTransition;
        }

        Status = MaintenanceStatus.InProgress;

        return Result.Success();
    }

    public Result Complete(DateOnly completedDate, decimal cost)
    {
        if (Status != MaintenanceStatus.InProgress)
        {
            return MaintenanceOrderErrors.InvalidStatusTransition;
        }

        if (completedDate == default)
        {
            return MaintenanceOrderErrors.InvalidCompletedDate;
        }

        if (cost is < 0m or > MaxCost)
        {
            return MaintenanceOrderErrors.InvalidCost;
        }

        CompletedDate = completedDate;
        Cost = cost;
        Status = MaintenanceStatus.Completed;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            return MaintenanceOrderErrors.InvalidStatusTransition;
        }

        Status = MaintenanceStatus.Cancelled;

        return Result.Success();
    }

    private static bool TryNormalizeRequestNumber(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length is < RequestNumberMinLength or > RequestNumberMaxLength)
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

    private static bool TryNormalizeDescription(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > DescriptionMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
