using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Application.Admin.Dtos;

public sealed record TenantModuleLicenseDto(
    Guid TenantId,
    string ModuleCode,
    bool IsActive,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? ExpiresAt,
    Guid ChangedBy,
    string ChangeReason,
    int Version)
{
    internal static TenantModuleLicenseDto From(TenantModule tenantModule)
    {
        ArgumentNullException.ThrowIfNull(tenantModule);

        return new TenantModuleLicenseDto(
            tenantModule.TenantId,
            tenantModule.ModuleCode,
            tenantModule.IsActive,
            tenantModule.ActivatedAt,
            tenantModule.ExpiresAt,
            tenantModule.ChangedBy,
            tenantModule.ChangeReason,
            tenantModule.Version);
    }
}
