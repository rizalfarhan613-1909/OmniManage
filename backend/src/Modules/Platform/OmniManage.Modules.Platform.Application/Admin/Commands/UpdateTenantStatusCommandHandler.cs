using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Application.Admin.Commands;

public sealed class UpdateTenantStatusCommandHandler(
    ITenantRepository tenants,
    ITenantContext tenantContext,
    IAuditLogger auditLogger)
    : ICommandHandler<UpdateTenantStatusCommand, TenantStatusDto>
{
    public async Task<Result<TenantStatusDto>> Handle(
        UpdateTenantStatusCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.TenantId == Guid.Empty || command.TenantId != tenantContext.TenantId)
        {
            return TenantErrors.NotFound;
        }

        if (!Enum.TryParse<TenantStatus>(command.Status?.Trim(), ignoreCase: true, out var status))
        {
            return TenantErrors.InvalidStatus;
        }

        var tenant = await tenants
            .GetByIdForUpdateAsync(command.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (tenant is null)
        {
            return TenantErrors.NotFound;
        }

        if (tenant.Status == status)
        {
            return new TenantStatusDto(tenant.Id, tenant.Status.ToString());
        }

        var before = tenant.Status;

        var transition = status switch
        {
            TenantStatus.Active => tenant.Reactivate(),
            TenantStatus.Suspended => tenant.Suspend(),
            TenantStatus.Archived => tenant.Archive(),
            _ => TenantErrors.InvalidStatusTransition(tenant.Status, "change status"),
        };

        if (transition.IsFailure)
        {
            return transition.Error;
        }

        await tenants.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await auditLogger.LogAsync(
            new AuditEntry(
                Action: "platform.tenant.status_change",
                EntityType: nameof(Tenant),
                EntityId: tenant.Id.ToString(),
                Before: new { Status = before.ToString() },
                After: new { Status = tenant.Status.ToString() },
                TenantId: tenant.Id,
                ActorType: "Superadmin"),
            failClosed: true,
            cancellationToken).ConfigureAwait(false);

        return new TenantStatusDto(tenant.Id, tenant.Status.ToString());
    }
}
