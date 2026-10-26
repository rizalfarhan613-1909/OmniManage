using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.Finance.Application;
using OmniManage.Modules.Finance.Application.IntegrationEvents;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Infrastructure.Outbox;
using OmniManage.Modules.Finance.Infrastructure.Persistence;
using OmniManage.Modules.Finance.Infrastructure.Persistence.Inbox;
using OmniManage.Modules.Finance.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.Finance.Infrastructure;

/// <summary>
/// Finance exposes the sub-codes FIN_GL, FIN_AR and FIN_AP. FIN_GL is the base
/// capability registered here; FIN_AR depends on FIN_GL. Controllers and
/// consumers declare the specific sub-code they require.
/// </summary>
public sealed class FinanceModule : IModule
{
    private static readonly string[] PlatformHardDependencies = [ModuleCodes.PlatformCore];

    public string ModuleCode => ModuleCodes.FinGl;

    public IReadOnlyCollection<string> HardDependencies => PlatformHardDependencies;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<FinanceDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, FinanceDbContext.Schema);
            options.AddTenantIsolation(serviceProvider.GetRequiredService<ITenantContext>());
        });

        services.AddFinanceApplication();
        RegisterIntegrationEventHandlers(services);

        services.AddScoped<IGlAccountRepository, GlAccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IArInvoiceRepository, ArInvoiceRepository>();
        services.AddScoped<IFinanceUnitOfWork, FinanceUnitOfWork>();
        services.AddScoped<IFinanceInbox, FinanceInbox>();
        services.AddScoped<IFinanceOutbox, FinanceOutbox>();
    }

    public void RegisterIntegrationEventHandlers(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddFinanceIntegrationEventHandlers();
    }
}
