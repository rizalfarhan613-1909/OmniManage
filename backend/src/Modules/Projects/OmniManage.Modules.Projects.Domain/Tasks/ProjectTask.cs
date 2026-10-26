using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Projects.Domain.Errors;

namespace OmniManage.Modules.Projects.Domain.Tasks;

public sealed class ProjectTask : Entity<Guid>, ITenantEntity
{
    public const int TitleMaxLength = 200;
    public const decimal MaxHours = 999_999.99m;

    private ProjectTask()
    {
    }

    private ProjectTask(
        Guid id,
        Guid tenantId,
        Guid projectId,
        string title,
        Guid? assigneeEmployeeId,
        decimal estimatedHours)
        : base(id)
    {
        TenantId = tenantId;
        ProjectId = projectId;
        Title = title;
        AssigneeEmployeeId = assigneeEmployeeId;
        EstimatedHours = estimatedHours;
        CompletedHours = 0m;
        Status = ProjectTaskStatus.Todo;
    }

    public Guid TenantId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public Guid? AssigneeEmployeeId { get; private set; }

    public ProjectTaskStatus Status { get; private set; }

    public decimal EstimatedHours { get; private set; }

    public decimal CompletedHours { get; private set; }

    public static Result<ProjectTask> Create(
        Guid id,
        Guid tenantId,
        Guid projectId,
        string? title,
        Guid? assigneeEmployeeId,
        decimal estimatedHours)
    {
        if (id == Guid.Empty)
        {
            return ProjectTaskErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return ProjectTaskErrors.InvalidTenantId;
        }

        if (projectId == Guid.Empty)
        {
            return ProjectTaskErrors.InvalidProjectId;
        }

        if (!TryNormalizeTitle(title, out var normalizedTitle))
        {
            return ProjectTaskErrors.InvalidTitle;
        }

        if (assigneeEmployeeId == Guid.Empty)
        {
            return ProjectTaskErrors.InvalidAssigneeEmployeeId;
        }

        if (estimatedHours is < 0m or > MaxHours)
        {
            return ProjectTaskErrors.InvalidEstimatedHours;
        }

        return new ProjectTask(
            id,
            tenantId,
            projectId,
            normalizedTitle,
            assigneeEmployeeId,
            estimatedHours);
    }

    public Result Assign(Guid employeeId)
    {
        if (Status == ProjectTaskStatus.Done)
        {
            return ProjectTaskErrors.InvalidStatusTransition;
        }

        if (employeeId == Guid.Empty)
        {
            return ProjectTaskErrors.InvalidAssigneeEmployeeId;
        }

        AssigneeEmployeeId = employeeId;

        return Result.Success();
    }

    public Result Start()
    {
        if (Status is not (ProjectTaskStatus.Todo or ProjectTaskStatus.Blocked))
        {
            return ProjectTaskErrors.InvalidStatusTransition;
        }

        Status = ProjectTaskStatus.InProgress;

        return Result.Success();
    }

    public Result Block()
    {
        if (Status != ProjectTaskStatus.InProgress)
        {
            return ProjectTaskErrors.InvalidStatusTransition;
        }

        Status = ProjectTaskStatus.Blocked;

        return Result.Success();
    }

    public Result RecordProgress(decimal hours)
    {
        if (hours is <= 0m or > MaxHours)
        {
            return ProjectTaskErrors.InvalidHours;
        }

        if (Status == ProjectTaskStatus.Done)
        {
            return ProjectTaskErrors.InvalidStatusTransition;
        }

        decimal completedHours;

        try
        {
            completedHours = decimal.Round(
                CompletedHours + hours,
                2,
                MidpointRounding.AwayFromZero);
        }
        catch (OverflowException)
        {
            return ProjectTaskErrors.InvalidCompletedHours;
        }

        if (completedHours > MaxHours)
        {
            return ProjectTaskErrors.InvalidCompletedHours;
        }

        CompletedHours = completedHours;

        if (Status == ProjectTaskStatus.Todo)
        {
            Status = ProjectTaskStatus.InProgress;
        }

        return Result.Success();
    }

    public Result Complete()
    {
        if (Status is not (ProjectTaskStatus.InProgress or ProjectTaskStatus.Blocked))
        {
            return ProjectTaskErrors.InvalidStatusTransition;
        }

        Status = ProjectTaskStatus.Done;

        return Result.Success();
    }

    private static bool TryNormalizeTitle(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > TitleMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
