namespace OmniManage.Modules.AssetMaintenance.Presentation.Controllers;

public sealed record CreateMaintenanceOrderRequest(
    string? RequestNumber,
    DateOnly? ScheduledDate,
    string? Description,
    string? MaintenanceType,
    string? Priority);
