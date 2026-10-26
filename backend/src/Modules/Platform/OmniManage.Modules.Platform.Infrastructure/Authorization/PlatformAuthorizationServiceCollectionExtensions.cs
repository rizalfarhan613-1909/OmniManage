using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Infrastructure.Authorization;

public static class PlatformAuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}
