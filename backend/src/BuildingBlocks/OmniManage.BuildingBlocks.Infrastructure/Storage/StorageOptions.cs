namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

/// <summary>
/// Configuration for tenant-isolated object storage.
/// </summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public StorageProvider Provider { get; init; } = StorageProvider.Local;

    public string BucketName { get; init; } = string.Empty;

    public string Region { get; init; } = "us-east-1";

    /// <summary>
    /// Gets the optional S3-compatible endpoint (MinIO, Cloudflare R2, ...).
    /// When omitted, the AWS regional endpoint is used.
    /// </summary>
    public string? ServiceUrl { get; init; }

    /// <summary>
    /// Gets the explicit access key. When both <see cref="AccessKey"/> and
    /// <see cref="SecretKey"/> are omitted, the AWS default credential chain is used.
    /// </summary>
    public string? AccessKey { get; init; }

    public string? SecretKey { get; init; }

    /// <summary>
    /// Gets a value indicating whether path-style addressing is used. S3-compatible
    /// providers such as MinIO generally require path-style addressing.
    /// </summary>
    public bool ForcePathStyle { get; init; } = true;

    /// <summary>
    /// Gets the root directory used by the <see cref="StorageProvider.Local"/> fallback.
    /// </summary>
    public string LocalRootPath { get; init; } = "App_Data/storage";
}
