namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

internal static class StorageGuard
{
    private static readonly TimeSpan MaximumPreSignedUrlLifetime = TimeSpan.FromDays(7);

    public static void ValidateUpload(
        Guid tenantId,
        string moduleCode,
        Stream content,
        string fileName,
        string contentType)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("A tenant identifier is required.", nameof(tenantId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(moduleCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanRead)
        {
            throw new ArgumentException("The content stream must be readable.", nameof(content));
        }
    }

    public static void ValidatePreSignedUrlRequest(TimeSpan expiresIn)
    {
        if (expiresIn <= TimeSpan.Zero || expiresIn > MaximumPreSignedUrlLifetime)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresIn),
                expiresIn,
                $"The pre-signed URL lifetime must be greater than zero and at most {MaximumPreSignedUrlLifetime.TotalDays} days.");
        }
    }
}
