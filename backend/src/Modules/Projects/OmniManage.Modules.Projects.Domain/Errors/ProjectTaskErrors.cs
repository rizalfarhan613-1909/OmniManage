using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Projects.Domain.Errors;

public static class ProjectTaskErrors
{
    public static readonly Error InvalidId = new(
        "PROJECTS.TASK.INVALID_ID",
        "A project task identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PROJECTS.TASK.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidProjectId = new(
        "PROJECTS.TASK.INVALID_PROJECT_ID",
        "A project identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTitle = new(
        "PROJECTS.TASK.INVALID_TITLE",
        "Task title is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidAssigneeEmployeeId = new(
        "PROJECTS.TASK.INVALID_ASSIGNEE_EMPLOYEE_ID",
        "The assignee must be a valid employee identifier.",
        ErrorType.Validation);

    public static readonly Error InvalidEstimatedHours = new(
        "PROJECTS.TASK.INVALID_ESTIMATED_HOURS",
        "Estimated hours must not be negative or exceed the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidHours = new(
        "PROJECTS.TASK.INVALID_HOURS",
        "Logged hours must be greater than zero and within the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidCompletedHours = new(
        "PROJECTS.TASK.INVALID_COMPLETED_HOURS",
        "Completed hours exceed the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusTransition = new(
        "PROJECTS.TASK.INVALID_STATUS_TRANSITION",
        "The requested task status transition is not allowed from the current status.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "PROJECTS.TASK.NOT_FOUND",
        "The project task does not exist in the current tenant.",
        ErrorType.NotFound);
}
