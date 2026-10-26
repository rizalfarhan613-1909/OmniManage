using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.AssetMaintenance.Application.Maintenance.RequestMaintenance;

public sealed record RequestMaintenanceCommand(
    Guid AssetId,
    string? RequestNumber,
    DateOnly? ScheduledDate,
    string? Description,
    string? MaintenanceType,
    string? Priority) : ICommand<Guid>
{
    public const string DefaultMaintenanceType = "CORRECTIVE";

    public const string DefaultPriority = "NORMAL";
}
