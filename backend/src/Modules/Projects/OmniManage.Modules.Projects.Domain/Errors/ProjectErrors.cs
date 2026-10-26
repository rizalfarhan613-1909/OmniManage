using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Projects.Domain.Errors;

public static class ProjectErrors
{
    public static readonly Error InvalidId = new(
        "PROJECTS.PROJECT.INVALID_ID",
        "A project identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PROJECTS.PROJECT.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "PROJECTS.PROJECT.INVALID_CODE",
        "Project code must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "PROJECTS.PROJECT.INVALID_NAME",
        "Project name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidDescription = new(
        "PROJECTS.PROJECT.INVALID_DESCRIPTION",
        "Project description must not exceed 1000 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidCustomerId = new(
        "PROJECTS.PROJECT.INVALID_CUSTOMER_ID",
        "A customer identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidStartDate = new(
        "PROJECTS.PROJECT.INVALID_START_DATE",
        "A valid project start date is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEndDate = new(
        "PROJECTS.PROJECT.INVALID_END_DATE",
        "Project end date must not be earlier than the start date.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusTransition = new(
        "PROJECTS.PROJECT.INVALID_STATUS_TRANSITION",
        "The requested project status transition is not allowed from the current status.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "PROJECTS.PROJECT.NOT_FOUND",
        "The project does not exist in the current tenant.",
        ErrorType.NotFound);
}
