using Microsoft.Extensions.DependencyInjection;

namespace OmniManage.Modules.MasterData.Application;

public static class MasterDataApplicationDependencyInjection
{
    public static IServiceCollection AddMasterDataApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(MasterDataApplicationDependencyInjection).Assembly));

        return services;
    }
}
