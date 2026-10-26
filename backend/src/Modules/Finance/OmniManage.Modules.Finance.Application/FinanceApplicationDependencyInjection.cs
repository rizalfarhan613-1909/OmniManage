using Microsoft.Extensions.DependencyInjection;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.AssetMaintenance.Contracts.IntegrationEvents;
using OmniManage.Modules.Finance.Application.IntegrationEventHandlers;
using OmniManage.Modules.Hris.Contracts.IntegrationEvents;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Finance.Application;

public static class FinanceApplicationDependencyInjection
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(FinanceApplicationDependencyInjection).Assembly));

        return services;
    }

    public static IServiceCollection AddFinanceIntegrationEventHandlers(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Register<SalesOrderConfirmedIntegrationEventHandler, SalesOrderConfirmedIntegrationEvent>(services);
        Register<PayrollFinalizedIntegrationEventHandler, PayrollFinalizedIntegrationEvent>(services);
        Register<AssetMaintenanceRequestedIntegrationEventHandler, AssetMaintenanceRequestedIntegrationEvent>(services);

        return services;
    }

    private static void Register<THandler, TEvent>(IServiceCollection services)
        where THandler : class, IIntegrationEventHandler<TEvent>
        where TEvent : IIntegrationEvent
    {
        services.AddScoped<THandler>();
        services.AddScoped<IIntegrationEventHandler<TEvent>>(
            serviceProvider => serviceProvider.GetRequiredService<THandler>());
        services.AddScoped<IIntegrationEventHandler>(
            serviceProvider => serviceProvider.GetRequiredService<THandler>());
    }
}
