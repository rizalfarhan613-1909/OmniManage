using Microsoft.Extensions.DependencyInjection;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.Modules.Inventory.Application.IntegrationEventHandlers;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Inventory.Application;

public static class InventoryApplicationDependencyInjection
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(InventoryApplicationDependencyInjection).Assembly));

        services.AddScoped<SalesOrderConfirmedIntegrationEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SalesOrderConfirmedIntegrationEvent>>(
            serviceProvider => serviceProvider.GetRequiredService<SalesOrderConfirmedIntegrationEventHandler>());
        services.AddScoped<IIntegrationEventHandler>(
            serviceProvider => serviceProvider.GetRequiredService<SalesOrderConfirmedIntegrationEventHandler>());

        return services;
    }
}
