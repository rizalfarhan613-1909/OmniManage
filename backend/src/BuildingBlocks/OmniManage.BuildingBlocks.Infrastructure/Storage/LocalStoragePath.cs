namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

internal static class LocalStoragePath
{
    public static string Resolve(string rootPath, Guid tenantId, string fileKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var relativePath = fileKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        // Defensive check: the key is validated before it reaches this method, but the
        // resolved path must never escape the configured storage root.
        if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, comparison))
        {
            throw new StorageTenantIsolationException(tenantId, fileKey);
        }

        return fullPath;
    }
}
