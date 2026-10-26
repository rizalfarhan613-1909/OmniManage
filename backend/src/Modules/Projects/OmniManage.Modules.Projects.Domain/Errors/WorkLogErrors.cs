using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Projects.Domain.Errors;

public static class WorkLogErrors
{
    public static readonly Error InvalidId = new(
        "PROJECTS.WORK_LOG.INVALID_ID",
        "A work log identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PROJECTS.WORK_LOG.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidProjectTaskId = new(
        "PROJECTS.WORK_LOG.INVALID_PROJECT_TASK_ID",
        "A project task identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEmployeeId = new(
        "PROJECTS.WORK_LOG.INVALID_EMPLOYEE_ID",
        "An employee identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidLoggedDate = new(
        "PROJECTS.WORK_LOG.INVALID_LOGGED_DATE",
        "A valid work log date is required.",
        ErrorType.Validation);

    public static readonly Error InvalidHoursSpent = new(
        "PROJECTS.WORK_LOG.INVALID_HOURS_SPENT",
        "Hours spent must be greater than zero and must not exceed 24 in a single log entry.",
        ErrorType.Validation);

    public static readonly Error InvalidDescription = new(
        "PROJECTS.WORK_LOG.INVALID_DESCRIPTION",
        "Work log description is required and must not exceed 500 characters.",
        ErrorType.Validation);
}
