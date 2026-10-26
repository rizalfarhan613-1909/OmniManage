using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Interceptors;

public sealed class TenantSaveChangesInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyTenantRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantRules(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var tenantId = tenantContext.TenantId;

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            var tenantProperty = entry.Property(nameof(ITenantEntity.TenantId));

            switch (entry.State)
            {
                case EntityState.Added:
                    var addedTenantId = ReadGuid(tenantProperty.CurrentValue);
                    if (addedTenantId != Guid.Empty && addedTenantId != tenantId)
                    {
                        throw new TenantIsolationException();
                    }

                    tenantProperty.CurrentValue = tenantId;
                    break;

                case EntityState.Modified:
                    var originalTenantId = ReadGuid(tenantProperty.OriginalValue);
                    var modifiedTenantId = ReadGuid(tenantProperty.CurrentValue);

                    if (originalTenantId != tenantId || modifiedTenantId != tenantId)
                    {
                        throw new TenantIsolationException();
                    }

                    tenantProperty.CurrentValue = tenantId;
                    tenantProperty.IsModified = false;
                    break;

                case EntityState.Deleted:
                    if (ReadGuid(tenantProperty.OriginalValue) != tenantId)
                    {
                        throw new TenantIsolationException();
                    }

                    break;

                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }
        }
    }

    private static Guid ReadGuid(object? value) => value is Guid guid ? guid : Guid.Empty;
}
