namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

/// <summary>
/// Thrown when a storage key is malformed or does not belong to the supplied tenant.
/// </summary>
public sealed class StorageTenantIsolationException : Exception
{
    public StorageTenantIsolationException(Guid tenantId, string fileKey)
        : base($"The file key '{fileKey}' does not belong to tenant '{tenantId}'.")
    {
        TenantId = tenantId;
        FileKey = fileKey;
    }

    public Guid TenantId { get; }

    public string FileKey { get; }
}
