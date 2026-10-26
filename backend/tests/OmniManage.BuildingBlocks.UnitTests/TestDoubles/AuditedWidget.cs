using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.UnitTests.TestDoubles;

public sealed class AuditedWidget : ITenantEntity, IAuditable
{
    private AuditedWidget()
    {
    }

    public AuditedWidget(Guid id, Guid tenantId, string name, string sensitiveValue)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        SensitiveValue = sensitiveValue;
        CreatedAt = new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);
        CreatedBy = Guid.Parse("0192f5b4-8a00-7000-8000-000000000901");
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; set; } = string.Empty;

    [Sensitive]
    public string SensitiveValue { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }
}
