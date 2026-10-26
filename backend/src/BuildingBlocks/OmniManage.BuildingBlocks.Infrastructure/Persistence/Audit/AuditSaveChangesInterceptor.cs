using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;

public sealed class AuditSaveChangesInterceptor(
    IAuditContext auditContext,
    IGuidGenerator guidGenerator,
    TimeProvider clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Capture(DbContext? context)
    {
        if (context is null || context.Model.FindEntityType(typeof(AuditLog)) is null)
        {
            return;
        }

        var auditedEntries = context.ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is IAuditable
                && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();

        if (auditedEntries.Length == 0)
        {
            return;
        }

        var occurredAt = clock.GetUtcNow();
        var auditLogs = context.Set<AuditLog>();

        foreach (var entry in auditedEntries)
        {
            var auditEntry = AuditEntryFactory.From(entry, auditContext);

            if (auditEntry is null)
            {
                continue;
            }

            auditLogs.Add(AuditLogFactory.Create(
                guidGenerator.NewGuid(),
                auditEntry,
                auditContext,
                occurredAt));
        }
    }
}
