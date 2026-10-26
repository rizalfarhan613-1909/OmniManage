using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;

public static class AuditEntryFactory
{
    public static AuditEntry? From(EntityEntry entry, IAuditContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(context);

        if (entry.Entity is not IAuditable)
        {
            return null;
        }

        var entityType = entry.Metadata.ClrType;
        var entityTypeName = entityType.Name;
        var operation = GetOperation(entry.State);

        if (operation is null)
        {
            return null;
        }

        var before = entry.State is EntityState.Modified or EntityState.Deleted
            ? ReadValues(entry, useOriginalValues: true, changedOnly: entry.State == EntityState.Modified)
            : null;
        var after = entry.State is EntityState.Added or EntityState.Modified
            ? ReadValues(entry, useOriginalValues: false, changedOnly: entry.State == EntityState.Modified)
            : null;

        if ((before is null || before.Count == 0) && (after is null || after.Count == 0))
        {
            return null;
        }

        return new AuditEntry(
            Action: $"{ToSnakeCase(entityTypeName)}.{operation}",
            ModuleCode: ResolveModuleCode(entityType),
            EntityType: entityTypeName,
            EntityId: ReadEntityId(entry),
            Before: before,
            After: after,
            TenantId: entry.Entity is ITenantEntity tenantEntity ? tenantEntity.TenantId : context.TenantId);
    }

    private static string? GetOperation(EntityState state) =>
        state switch
        {
            EntityState.Added => "created",
            EntityState.Modified => "updated",
            EntityState.Deleted => "deleted",
            _ => null,
        };

    private static Dictionary<string, object?> ReadValues(
        EntityEntry entry,
        bool useOriginalValues,
        bool changedOnly)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty())
            {
                continue;
            }

            if (changedOnly && !property.IsModified)
            {
                continue;
            }

            var value = useOriginalValues ? property.OriginalValue : property.CurrentValue;
            var propertyName = property.Metadata.Name;

            values[propertyName] = IsSensitive(property) ? AuditJson.RedactedValue : value;
        }

        return values;
    }

    private static string? ReadEntityId(EntityEntry entry)
    {
        var keyValues = entry.Properties
            .Where(property => property.Metadata.IsPrimaryKey())
            .Select(property => property.CurrentValue?.ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        return keyValues.Length == 0 ? null : string.Join(':', keyValues);
    }

    private static bool IsSensitive(PropertyEntry property) =>
        property.Metadata.PropertyInfo?.GetCustomAttribute<SensitiveAttribute>() is not null
        || AuditJson.IsSensitiveName(property.Metadata.Name);

    private static string? ResolveModuleCode(Type entityType) =>
        entityType.Namespace?.StartsWith("OmniManage.Modules.Platform", StringComparison.Ordinal) == true
            ? ModuleCodes.PlatformCore
            : null;

    private static string ToSnakeCase(string value)
    {
        var builder = new StringBuilder(value.Length + 8);

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];

            if (char.IsUpper(character) && index > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}
