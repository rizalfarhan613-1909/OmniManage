using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;

namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

/// <summary>
/// Local file system fallback for development and integration tests. It implements
/// the same tenant-isolated key convention as <see cref="S3StorageService"/> so
/// switching providers does not change callers or stored file keys.
/// </summary>
/// <remarks>
/// ASSUMPTION: local storage has no HTTP endpoint or signing infrastructure. Pre-signed
/// URL requests therefore return a <c>file://</c> URI pointing at the local file. This is
/// only meaningful on the machine hosting the API and must never be used in production.
/// </remarks>
public sealed class LocalFileStorageService : IStorageService
{
    private readonly StorageOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidGenerator _guidGenerator;

    public LocalFileStorageService(
        IOptions<StorageOptions> options,
        TimeProvider timeProvider,
        IGuidGenerator guidGenerator)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));

        if (string.IsNullOrWhiteSpace(_options.LocalRootPath))
        {
            throw new InvalidOperationException(
                "Storage:LocalRootPath must be configured when Storage:Provider is Local.");
        }
    }

    public async Task<string> UploadFileAsync(
        Guid tenantId,
        string moduleCode,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        StorageGuard.ValidateUpload(tenantId, moduleCode, content, fileName, contentType);

        var fileKey = StorageObjectKey.Create(
            tenantId,
            moduleCode,
            fileName,
            _timeProvider.GetUtcNow(),
            _guidGenerator.NewGuid());

        var path = LocalStoragePath.Resolve(_options.LocalRootPath, tenantId, fileKey);
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"The local storage path '{path}' has no directory.");

        Directory.CreateDirectory(directory);

        await using var destination = new FileStream(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                BufferSize = 81_920,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });

        await content.CopyToAsync(destination, ct).ConfigureAwait(false);

        return fileKey;
    }

    public Task<string> GetPreSignedUrlAsync(
        Guid tenantId,
        string fileKey,
        TimeSpan expiresIn,
        CancellationToken ct = default)
    {
        var ownedKey = StorageObjectKey.EnsureTenantOwnership(tenantId, fileKey);
        StorageGuard.ValidatePreSignedUrlRequest(expiresIn);
        ct.ThrowIfCancellationRequested();

        var path = LocalStoragePath.Resolve(_options.LocalRootPath, tenantId, ownedKey);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The requested file does not exist.", path);
        }

        return Task.FromResult(new Uri(path).AbsoluteUri);
    }

    public Task DeleteFileAsync(
        Guid tenantId,
        string fileKey,
        CancellationToken ct = default)
    {
        var ownedKey = StorageObjectKey.EnsureTenantOwnership(tenantId, fileKey);
        ct.ThrowIfCancellationRequested();

        var path = LocalStoragePath.Resolve(_options.LocalRootPath, tenantId, ownedKey);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }
}
