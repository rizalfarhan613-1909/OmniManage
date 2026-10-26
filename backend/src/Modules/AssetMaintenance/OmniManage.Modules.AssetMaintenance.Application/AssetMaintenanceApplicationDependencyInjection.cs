using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.BuildingBlocks.Application.Validation;
using OmniManage.Modules.AssetMaintenance.Application.Assets.CreateAsset;
using OmniManage.Modules.AssetMaintenance.Application.Maintenance.RequestMaintenance;

namespace OmniManage.Modules.AssetMaintenance.Application;

public static class AssetMaintenanceApplicationDependencyInjection
{
    public static IServiceCollection AddAssetMaintenanceApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(
                typeof(AssetMaintenanceApplicationDependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IValidator<CreateAssetCommand>, CreateAssetValidator>();
        services.AddScoped<IValidator<RequestMaintenanceCommand>, RequestMaintenanceValidator>();

        return services;
    }
}
