namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

/// <summary>
/// Selects the backing implementation of <c>IStorageService</c>.
/// </summary>
public enum StorageProvider
{
    /// <summary>
    /// Local file system fallback used for development and integration tests.
    /// </summary>
    Local = 0,

    /// <summary>
    /// S3-compatible object storage (AWS S3, MinIO, Cloudflare R2, ...).
    /// </summary>
    S3 = 1,
}
