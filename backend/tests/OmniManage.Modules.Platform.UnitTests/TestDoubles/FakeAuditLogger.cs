using OmniManage.BuildingBlocks.Application.Auditing;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeAuditLogger : IAuditLogger
{
    public List<AuditEntry> Entries { get; } = [];

    public List<bool> FailClosedFlags { get; } = [];

    public Task LogAsync(
        AuditEntry entry,
        bool failClosed = true,
        CancellationToken cancellationToken = default)
    {
        Entries.Add(entry);
        FailClosedFlags.Add(failClosed);
        return Task.CompletedTask;
    }
}
