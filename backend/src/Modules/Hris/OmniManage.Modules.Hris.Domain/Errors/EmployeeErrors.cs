using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Hris.Domain.Errors;

public static class EmployeeErrors
{
    public static readonly Error InvalidId = new(
        "HRIS.EMPLOYEE.INVALID_ID",
        "An employee identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "HRIS.EMPLOYEE.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEmployeeCode = new(
        "HRIS.EMPLOYEE.INVALID_EMPLOYEE_CODE",
        "Employee code must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.",
        ErrorType.Validation);

    public static readonly Error InvalidFullName = new(
        "HRIS.EMPLOYEE.INVALID_FULL_NAME",
        "Employee full name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidEmail = new(
        "HRIS.EMPLOYEE.INVALID_EMAIL",
        "Employee email is required, must be a valid address and must not exceed 320 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidJobTitle = new(
        "HRIS.EMPLOYEE.INVALID_JOB_TITLE",
        "Employee job title is required and must not exceed 100 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidDepartmentId = new(
        "HRIS.EMPLOYEE.INVALID_DEPARTMENT_ID",
        "A department identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidJoinedDate = new(
        "HRIS.EMPLOYEE.INVALID_JOINED_DATE",
        "A valid joining date is required.",
        ErrorType.Validation);

    public static readonly Error AlreadyActive = new(
        "HRIS.EMPLOYEE.ALREADY_ACTIVE",
        "The employee is already active.",
        ErrorType.Conflict);

    public static readonly Error AlreadyInactive = new(
        "HRIS.EMPLOYEE.ALREADY_INACTIVE",
        "The employee is already inactive.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "HRIS.EMPLOYEE.NOT_FOUND",
        "The employee does not exist in the current tenant.",
        ErrorType.NotFound);
}
