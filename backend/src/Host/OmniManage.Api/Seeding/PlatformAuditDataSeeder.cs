using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;
using OmniManage.Modules.Platform.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Adds a small, hash-chained audit trail so the platform audit-log workspace
/// has realistic content on a fresh demo database. Entries use stable trace ids
/// to stay idempotent across restarts.
/// </summary>
internal static class PlatformAuditDataSeeder
{
    private const string TraceIdPrefix = "SEED-AUDIT-";

    private static readonly AuditSample[] Samples =
    [
        new("platform.tenant_module.activate", ModuleCodes.PlatformCore, "TenantModule", "SALES_CORE",
            new() { ["active"] = false }, new() { ["active"] = true }),
        new("platform.tenant_module.activate", ModuleCodes.PlatformCore, "TenantModule", "FIN_AR",
            new() { ["active"] = false }, new() { ["active"] = true }),
        new("platform.tenant.status.read", ModuleCodes.PlatformCore, "Tenant", "TEN-8492", null,
            new() { ["status"] = "Active" }),
        new("platform.session.terminate", ModuleCodes.PlatformCore, "RefreshToken", "session-demo-01",
            new() { ["status"] = "Active" }, new() { ["status"] = "Revoked" }),
        new("sales.order.confirm", ModuleCodes.SalesCore, "SalesOrder", "SO-000001",
            new() { ["status"] = "Draft" }, new() { ["status"] = "Confirmed" }),
        new("sales.order.confirm", ModuleCodes.SalesCore, "SalesOrder", "SO-000002",
            new() { ["status"] = "Draft" }, new() { ["status"] = "Confirmed" }),
        new("finance.invoice.paid", ModuleCodes.FinAr, "ArInvoice", "AR-000001",
            new() { ["status"] = "Unpaid" }, new() { ["status"] = "Paid" }),
        new("finance.journal.post", ModuleCodes.FinGl, "JournalEntry", "JE-AR-000001", null,
            new() { ["debitTotal"] = "1899.0000", ["creditTotal"] = "1899.0000" }),
        new("inventory.stock.receive", ModuleCodes.InvCore, "StockLedger", "SEED-RECEIVE-01", null,
            new() { ["quantityDelta"] = "30.0000" }),
        new("inventory.stock.issue", ModuleCodes.InvCore, "StockLedger", "SEED-ISSUE-01",
            new() { ["quantityDelta"] = "30.0000" }, new() { ["quantityDelta"] = "25.0000" }),
        new("masterdata.item.update", ModuleCodes.CoreMasterData, "Item", "LAPTOP-PRO-14",
            new() { ["basePrice"] = "1799.0000" }, new() { ["basePrice"] = "1899.0000" }),
        new("procurement.purchase_order.create", ModuleCodes.ProcCore, "PurchaseOrder", "PO-000001", null,
            new() { ["status"] = "Draft", ["totalAmount"] = "14500.0000" }),
        new("manufacturing.work_order.create", ModuleCodes.MfgCore, "WorkOrder", "WO-000001", null,
            new() { ["status"] = "Draft", ["quantityPlanned"] = "10.000000" }),
        new("hris.employee.update", ModuleCodes.HrisCore, "Employee", "EMP-0001",
            new() { ["jobTitle"] = "Analyst" }, new() { ["jobTitle"] = "Senior Analyst" }),
        new("hris.attendance.check_in", ModuleCodes.HrisAttendance, "AttendanceLog", "ATT-000001", null,
            new() { ["status"] = "Present" }),
        new("projects.task.complete", ModuleCodes.ProjectCore, "ProjectTask", "TASK-0001",
            new() { ["status"] = "InProgress" }, new() { ["status"] = "Completed" }),
        new("assets.maintenance.create", ModuleCodes.AssetCore, "MaintenanceOrder", "MRO-000001", null,
            new() { ["status"] = "Open", ["priority"] = "High" }),
        new("crm.customer.read", ModuleCodes.CrmCore, "BusinessPartner", "CUST-0001", null,
            new() { ["creditLimit"] = "250000.0000" }),
    ];

    public static async Task SeedAsync(
        PlatformDbContext platform,
        Guid tenantId,
        Guid actorId,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(platform);
        ArgumentNullException.ThrowIfNull(guidGenerator);

        var knownTraceIds = await platform.AuditLogs
            .AsNoTracking()
            .Where(log => log.TenantId == tenantId && log.TraceId != null
                && log.TraceId.StartsWith(TraceIdPrefix))
            .Select(log => log.TraceId!)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (knownTraceIds.Count >= Samples.Length)
        {
            return;
        }

        var previousHash = await platform.AuditLogs
            .AsNoTracking()
            .Where(log => log.TenantId == tenantId)
            .OrderByDescending(log => log.OccurredAt)
            .ThenByDescending(log => log.Id)
            .Select(log => log.RowHash)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var knownTraces = knownTraceIds.ToHashSet(StringComparer.Ordinal);
        var auditContext = new SeedAuditContext(tenantId, actorId);
        var firstOccurrence = now.AddDays(-14);

        for (var index = 0; index < Samples.Length; index++)
        {
            var traceId = $"{TraceIdPrefix}{index + 1:D2}";

            if (!knownTraces.Add(traceId))
            {
                continue;
            }

            var sample = Samples[index];
            var occurredAt = firstOccurrence.AddHours(index * 16);
            var entry = new AuditEntry(
                sample.Action,
                sample.ModuleCode,
                sample.EntityType,
                sample.EntityId,
                sample.Before,
                sample.After,
                TenantId: tenantId,
                ActorId: actorId,
                ActorType: "User",
                IpAddress: "203.0.113.42",
                UserAgent: "OmniManage demo seed",
                TraceId: traceId);

            var log = AuditLogFactory.Create(
                guidGenerator.NewGuid(),
                entry,
                auditContext,
                occurredAt,
                previousHash);

            platform.AuditLogs.Add(log);
            previousHash = log.RowHash;
        }

        await platform.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private sealed record AuditSample(
        string Action,
        string ModuleCode,
        string EntityType,
        string EntityId,
        Dictionary<string, object?>? Before,
        Dictionary<string, object?>? After);

    private sealed class SeedAuditContext(Guid tenantId, Guid actorId) : IAuditContext
    {
        public Guid? TenantId { get; } = tenantId;

        public Guid? ActorId { get; } = actorId;

        public string ActorType => "User";

        public Guid? ImpersonatedBy => null;

        public string? IpAddress => "203.0.113.42";

        public string? UserAgent => "OmniManage demo seed";

        public string? TraceId => null;
    }
}
