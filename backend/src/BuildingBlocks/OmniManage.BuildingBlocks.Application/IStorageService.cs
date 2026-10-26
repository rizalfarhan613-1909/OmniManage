namespace OmniManage.BuildingBlocks.Application;

/// <summary>
/// Abstraction over S3/MinIO compatible object storage.
/// Implementations must isolate files per tenant and must never expose
/// direct public bucket access. Files are stored using the path convention
/// <c>{tenant_id}/{module_code}/{year}/{month}/{file_id}.ext</c> and are
/// only accessible through short-lived pre-signed URLs.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file for the specified tenant and module.
    /// </summary>
    /// <returns>The storage file key that can later be used to read or delete the file.</returns>
    Task<string> UploadFileAsync(
        Guid tenantId,
        string moduleCode,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a short-lived pre-signed URL for an existing file.
    /// </summary>
    Task<string> GetPreSignedUrlAsync(
        Guid tenantId,
        string fileKey,
        TimeSpan expiresIn,
        CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes a file owned by the specified tenant.
    /// </summary>
    Task DeleteFileAsync(
        Guid tenantId,
        string fileKey,
        CancellationToken ct = default);
}
