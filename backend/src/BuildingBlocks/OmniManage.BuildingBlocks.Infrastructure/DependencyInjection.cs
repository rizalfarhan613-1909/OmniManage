using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Export;
using OmniManage.BuildingBlocks.Infrastructure.Storage;

namespace OmniManage.BuildingBlocks.Infrastructure;

/// <summary>
/// Registers the building-block infrastructure services shared by every module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddOmniManageInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOmniManageStorage(configuration);
        services.AddOmniManageExportEngine();

        return services;
    }

    public static IServiceCollection AddOmniManageStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .Validate(
                options => Enum.IsDefined(typeof(StorageProvider), options.Provider),
                "Storage:Provider must be 'Local' or 'S3'.")
            .Validate(
                options => options.Provider != StorageProvider.S3 ||
                           !string.IsNullOrWhiteSpace(options.BucketName),
                "Storage:BucketName must be configured when Storage:Provider is S3.")
            .Validate(
                options => options.Provider != StorageProvider.S3 ||
                           string.IsNullOrWhiteSpace(options.ServiceUrl) ||
                           Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out _),
                "Storage:ServiceUrl must be an absolute URL when configured.")
            .Validate(
                options => options.Provider != StorageProvider.S3 ||
                           !string.IsNullOrWhiteSpace(options.Region),
                "Storage:Region must be configured when Storage:Provider is S3.")
            .Validate(
                options => (string.IsNullOrWhiteSpace(options.AccessKey) &&
                            string.IsNullOrWhiteSpace(options.SecretKey)) ||
                           (!string.IsNullOrWhiteSpace(options.AccessKey) &&
                            !string.IsNullOrWhiteSpace(options.SecretKey)),
                "Storage:AccessKey and Storage:SecretKey must be configured together.")
            .Validate(
                options => options.Provider != StorageProvider.Local ||
                           !string.IsNullOrWhiteSpace(options.LocalRootPath),
                "Storage:LocalRootPath must be configured when Storage:Provider is Local.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IGuidGenerator, GuidGenerator>();

        services.AddSingleton<IAmazonS3>(serviceProvider =>
            CreateS3Client(serviceProvider.GetRequiredService<IOptions<StorageOptions>>().Value));

        services.AddSingleton<IStorageService>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;

            return options.Provider switch
            {
                StorageProvider.S3 => ActivatorUtilities.CreateInstance<S3StorageService>(serviceProvider),
                StorageProvider.Local => ActivatorUtilities.CreateInstance<LocalFileStorageService>(serviceProvider),
                _ => throw new InvalidOperationException(
                    $"The storage provider '{options.Provider}' is not supported."),
            };
        });

        return services;
    }

    public static IServiceCollection AddOmniManageExportEngine(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IExportEngine, UniversalExportEngine>();

        return services;
    }

    private static IAmazonS3 CreateS3Client(StorageOptions options)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = options.ForcePathStyle,
        };

        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            config.ServiceURL = options.ServiceUrl;
            config.AuthenticationRegion = options.Region;
        }
        else
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
        }

        return string.IsNullOrWhiteSpace(options.AccessKey)
            ? new AmazonS3Client(config)
            : new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKey, options.SecretKey),
                config);
    }
}
