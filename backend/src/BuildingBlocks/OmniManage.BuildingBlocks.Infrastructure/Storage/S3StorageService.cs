using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;

namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

/// <summary>
/// S3-compatible implementation of <see cref="IStorageService"/>.
/// All keys are prefixed with the owning tenant identifier and files can only be
/// read through short-lived pre-signed URLs; the bucket itself stays private.
/// </summary>
public sealed class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidGenerator _guidGenerator;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<StorageOptions> options,
        TimeProvider timeProvider,
        IGuidGenerator guidGenerator)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));

        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException(
                "Storage:BucketName must be configured when Storage:Provider is S3.");
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

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey,
            InputStream = content,
            ContentType = contentType.Trim(),
            AutoCloseStream = false,
        };

        await _s3Client.PutObjectAsync(request, ct).ConfigureAwait(false);

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

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = ownedKey,
            Verb = HttpVerb.GET,
            Expires = _timeProvider.GetUtcNow().Add(expiresIn).UtcDateTime,
        };

        var url = _s3Client.GetPreSignedURL(request);

        return Task.FromResult(url);
    }

    public async Task DeleteFileAsync(
        Guid tenantId,
        string fileKey,
        CancellationToken ct = default)
    {
        var ownedKey = StorageObjectKey.EnsureTenantOwnership(tenantId, fileKey);

        // S3 deletes are idempotent: deleting a missing object succeeds.
        await _s3Client
            .DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = ownedKey,
                },
                ct)
            .ConfigureAwait(false);
    }
}
