using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Application.Admin.Dtos;

public sealed record TenantSummaryDto(
    Guid Id,
    string Code,
    string Name,
    string Status,
    string PlanCode,
    int ActiveUserCount,
    int ActiveUserQuota,
    IReadOnlyList<string> LicensedModules,
    DateTimeOffset CreatedAt)
{
    internal static TenantSummaryDto From(TenantSummaryProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);

        return new TenantSummaryDto(
            projection.Id,
            projection.Code,
            projection.Name,
            projection.Status.ToString(),
            projection.PlanCode,
            projection.ActiveUserCount,
            QuotaFor(projection.PlanCode),
            projection.LicensedModules,
            projection.CreatedAt);
    }

    private static int QuotaFor(string planCode) =>
        planCode.Trim().ToUpperInvariant() switch
        {
            "ENTERPRISE" => 500,
            "PRO" or "PROFESSIONAL" => 100,
            "STARTER" or "BASIC" => 25,
            _ => 50,
        };
}
