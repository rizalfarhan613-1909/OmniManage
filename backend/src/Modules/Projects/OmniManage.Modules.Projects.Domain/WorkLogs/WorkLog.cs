using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Projects.Domain.Errors;

namespace OmniManage.Modules.Projects.Domain.WorkLogs;

public sealed class WorkLog : Entity<Guid>, ITenantEntity
{
    public const int DescriptionMaxLength = 500;
    public const decimal MaxHoursPerLog = 24m;

    private WorkLog()
    {
    }

    private WorkLog(
        Guid id,
        Guid tenantId,
        Guid projectTaskId,
        Guid employeeId,
        DateOnly loggedDate,
        decimal hoursSpent,
        string description)
        : base(id)
    {
        TenantId = tenantId;
        ProjectTaskId = projectTaskId;
        EmployeeId = employeeId;
        LoggedDate = loggedDate;
        HoursSpent = hoursSpent;
        Description = description;
    }

    public Guid TenantId { get; private set; }

    public Guid ProjectTaskId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public DateOnly LoggedDate { get; private set; }

    public decimal HoursSpent { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public static Result<WorkLog> Create(
        Guid id,
        Guid tenantId,
        Guid projectTaskId,
        Guid employeeId,
        DateOnly loggedDate,
        decimal hoursSpent,
        string? description)
    {
        if (id == Guid.Empty)
        {
            return WorkLogErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return WorkLogErrors.InvalidTenantId;
        }

        if (projectTaskId == Guid.Empty)
        {
            return WorkLogErrors.InvalidProjectTaskId;
        }

        if (employeeId == Guid.Empty)
        {
            return WorkLogErrors.InvalidEmployeeId;
        }

        if (loggedDate == default)
        {
            return WorkLogErrors.InvalidLoggedDate;
        }

        if (hoursSpent is <= 0m or > MaxHoursPerLog)
        {
            return WorkLogErrors.InvalidHoursSpent;
        }

        if (!TryNormalizeDescription(description, out var normalizedDescription))
        {
            return WorkLogErrors.InvalidDescription;
        }

        return new WorkLog(
            id,
            tenantId,
            projectTaskId,
            employeeId,
            loggedDate,
            hoursSpent,
            normalizedDescription);
    }

    private static bool TryNormalizeDescription(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > DescriptionMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
