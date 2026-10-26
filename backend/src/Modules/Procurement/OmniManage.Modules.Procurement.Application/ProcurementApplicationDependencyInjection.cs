using Microsoft.Extensions.DependencyInjection;

namespace OmniManage.Modules.Procurement.Application;

public static class ProcurementApplicationDependencyInjection
{
    public static IServiceCollection AddProcurementApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(
                typeof(ProcurementApplicationDependencyInjection).Assembly));

        return services;
    }
}
