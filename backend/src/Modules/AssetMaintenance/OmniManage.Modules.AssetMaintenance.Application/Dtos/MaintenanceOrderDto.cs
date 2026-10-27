using System.Globalization;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Application.Dtos;

public sealed record MaintenanceOrderDto(
    Guid Id,
    Guid AssetId,
    string RequestNumber,
    DateOnly? ScheduledDate,
    DateOnly? CompletedDate,
    string Cost,
    string Status,
    string Description)
{
    internal static MaintenanceOrderDto From(MaintenanceOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new MaintenanceOrderDto(
            order.Id,
            order.AssetId,
            order.RequestNumber,
            order.ScheduledDate,
            order.CompletedDate,
            order.Cost.ToString("0.0000", CultureInfo.InvariantCulture),
            order.Status.ToString(),
            order.Description);
    }
}
