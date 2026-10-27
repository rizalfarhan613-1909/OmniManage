using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Application.Admin.Commands;

public sealed class ToggleTenantModuleCommandHandler(
    ITenantRepository tenants,
    ITenantModuleRepository tenantModules,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    ITenantModuleCache licenses,
    IAuditLogger auditLogger,
    TimeProvider clock)
    : ICommandHandler<ToggleTenantModuleCommand, TenantModuleLicenseDto>
{
    public async Task<Result<TenantModuleLicenseDto>> Handle(
        ToggleTenantModuleCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.TenantId == Guid.Empty || command.TenantId != tenantContext.TenantId)
        {
            return TenantErrors.NotFound;
        }

        var moduleCode = command.ModuleCode?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(moduleCode)
            || !ModuleCodes.All.Contains(moduleCode))
        {
            return TenantModuleErrors.InvalidModuleCode;
        }

        if (currentUser.UserId == Guid.Empty)
        {
            return TenantModuleErrors.InvalidChangedBy;
        }

        var tenant = await tenants
            .GetByIdAsync(command.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (tenant is null)
        {
            return TenantErrors.NotFound;
        }

        var now = clock.GetUtcNow();
        var existing = await tenantModules
            .GetAsync(command.TenantId, moduleCode, cancellationToken)
            .ConfigureAwait(false);

        TenantModule tenantModule;
        var beforeIsActive = existing?.IsActive ?? false;

        if (existing is null)
        {
            if (!command.IsActive)
            {
                return TenantModuleErrors.AlreadyInactive;
            }

            var created = TenantModule.CreateActive(
                command.TenantId,
                moduleCode,
                currentUser.UserId,
                command.Reason,
                now);

            if (created.IsFailure)
            {
                return created.Error;
            }

            tenantModule = created.Value;
            await tenantModules
                .AddAsync(tenantModule, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            var transition = command.IsActive
                ? existing.Activate(currentUser.UserId, command.Reason, now)
                : existing.Deactivate(currentUser.UserId, command.Reason);

            if (transition.IsFailure)
            {
                return transition.Error;
            }

            tenantModule = existing;
        }

        await tenantModules.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await licenses
            .InvalidateAsync(command.TenantId, cancellationToken)
            .ConfigureAwait(false);

        await auditLogger.LogAsync(
            new AuditEntry(
                Action: command.IsActive
                    ? "platform.tenant_module.activate"
                    : "platform.tenant_module.deactivate",
                ModuleCode: ModuleCodes.PlatformCore,
                EntityType: nameof(TenantModule),
                EntityId: $"{command.TenantId:D}:{moduleCode}",
                Before: new { IsActive = beforeIsActive },
                After: new
                {
                    IsActive = tenantModule.IsActive,
                    Reason = tenantModule.ChangeReason,
                    tenantModule.Version,
                },
                TenantId: command.TenantId,
                ActorType: "Superadmin"),
            failClosed: true,
            cancellationToken).ConfigureAwait(false);

        return TenantModuleLicenseDto.From(tenantModule);
    }
}
