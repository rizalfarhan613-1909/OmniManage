using System.Globalization;

namespace OmniManage.BuildingBlocks.Infrastructure.Storage;

/// <summary>
/// Builds and validates tenant-isolated storage keys using the path convention
/// <c>{tenant_id}/{module_code}/{year}/{month}/{file_id}.ext</c>.
/// </summary>
internal static class StorageObjectKey
{
    private const int MaxKeyLength = 1024;
    private const int MaxModuleCodeLength = 64;
    private const int MaxExtensionLength = 16;

    public static string Create(
        Guid tenantId,
        string moduleCode,
        string fileName,
        DateTimeOffset timestamp,
        Guid fileId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("A tenant identifier is required.", nameof(tenantId));
        }

        var normalizedModule = NormalizeModuleCode(moduleCode);
        var extension = GetExtension(fileName);
        var utc = timestamp.UtcDateTime;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{tenantId:D}/{normalizedModule}/{utc:yyyy}/{utc:MM}/{fileId:D}{extension}");
    }

    public static string EnsureTenantOwnership(Guid tenantId, string fileKey)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("A tenant identifier is required.", nameof(tenantId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fileKey);

        var isMalformed = fileKey.Length > MaxKeyLength ||
                          fileKey[0] == '/' ||
                          fileKey.Contains('\\', StringComparison.Ordinal) ||
                          fileKey.Contains("..", StringComparison.Ordinal) ||
                          fileKey.Any(char.IsControl);

        if (isMalformed)
        {
            throw new StorageTenantIsolationException(tenantId, fileKey);
        }

        var tenantPrefix = tenantId.ToString("D", CultureInfo.InvariantCulture) + "/";

        if (!fileKey.StartsWith(tenantPrefix, StringComparison.Ordinal))
        {
            throw new StorageTenantIsolationException(tenantId, fileKey);
        }

        return fileKey;
    }

    private static string NormalizeModuleCode(string moduleCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleCode);

        var normalized = moduleCode.Trim().ToUpperInvariant();

        if (normalized.Length > MaxModuleCodeLength)
        {
            throw new ArgumentException(
                $"The module code must not exceed {MaxModuleCodeLength} characters.",
                nameof(moduleCode));
        }

        foreach (var character in normalized)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '_')
            {
                throw new ArgumentException(
                    "The module code may only contain letters, digits, and underscores.",
                    nameof(moduleCode));
            }
        }

        return normalized;
    }

    private static string GetExtension(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var extension = Path.GetExtension(fileName);

        if (extension.Length is < 2 or > MaxExtensionLength + 1)
        {
            return string.Empty;
        }

        var candidate = extension[1..];

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character))
            {
                return string.Empty;
            }
        }

        return "." + candidate.ToLowerInvariant();
    }
}
