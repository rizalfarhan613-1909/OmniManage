using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence;

public static class TenantDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder AddTenantIsolation(
        this DbContextOptionsBuilder optionsBuilder,
        ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(tenantContext);

        optionsBuilder.AddInterceptors(
            new Interceptors.TenantSaveChangesInterceptor(tenantContext),
            new Interceptors.TenantConnectionInterceptor(tenantContext));

        return optionsBuilder;
    }

    public static DbContextOptionsBuilder<TContext> AddTenantIsolation<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        ITenantContext tenantContext)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        ((DbContextOptionsBuilder)optionsBuilder).AddTenantIsolation(tenantContext);

        return optionsBuilder;
    }
}
