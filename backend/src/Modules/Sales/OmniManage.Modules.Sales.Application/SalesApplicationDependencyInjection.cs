using Microsoft.Extensions.DependencyInjection;

namespace OmniManage.Modules.Sales.Application;

public static class SalesApplicationDependencyInjection
{
    public static IServiceCollection AddSalesApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(SalesApplicationDependencyInjection).Assembly));

        return services;
    }
}
