using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence;

public abstract class TenantDbContext(DbContextOptions options, ITenantContext tenantContext)
    : BaseDbContext(options)
{
    private static readonly MethodInfo SetTenantFilterMethod = typeof(TenantDbContext)
        .GetMethod(nameof(SetTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("The tenant query filter method could not be resolved.");

    protected ITenantContext TenantContext { get; } =
        tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            SetTenantFilterMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, [modelBuilder]);
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => entity.TenantId == TenantContext.TenantId);
    }
}
