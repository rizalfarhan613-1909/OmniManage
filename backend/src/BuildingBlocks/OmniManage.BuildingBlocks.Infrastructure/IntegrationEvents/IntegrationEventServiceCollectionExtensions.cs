using Microsoft.Extensions.DependencyInjection;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;

namespace OmniManage.BuildingBlocks.Infrastructure.IntegrationEvents;

public static class IntegrationEventServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationEventDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        return services;
    }
}
