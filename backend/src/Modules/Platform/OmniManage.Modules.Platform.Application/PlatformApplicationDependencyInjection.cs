using Microsoft.Extensions.DependencyInjection;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Application;

public static class PlatformApplicationDependencyInjection
{
    public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(PlatformApplicationDependencyInjection).Assembly));
        services.AddScoped<AuthSessionBuilder>();
        services.AddScoped<RefreshTokenSessionRevoker>();

        return services;
    }
}
