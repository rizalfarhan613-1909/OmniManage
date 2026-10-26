using Microsoft.Extensions.DependencyInjection;

namespace OmniManage.Modules.Manufacturing.Application;

public static class ManufacturingApplicationDependencyInjection
{
    public static IServiceCollection AddManufacturingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(
                typeof(ManufacturingApplicationDependencyInjection).Assembly));

        return services;
    }
}
