namespace OmniManage.BuildingBlocks.Application.Auditing;

public interface IAuditLogger
{
    /// <summary>
    /// Persists an explicit business/security audit entry.
    /// When <paramref name="failClosed"/> is true (the default), an audit failure propagates and
    /// aborts the calling operation. Security and financial actions must use the fail-closed mode.
    /// </summary>
    Task LogAsync(
        AuditEntry entry,
        bool failClosed = true,
        CancellationToken cancellationToken = default);
}
