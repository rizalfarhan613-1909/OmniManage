using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Hris.Domain.Errors;

namespace OmniManage.Modules.Hris.Domain.Employees;

public sealed class Employee : AggregateRoot<Guid>, ITenantEntity
{
    public const int EmployeeCodeMinLength = 2;
    public const int EmployeeCodeMaxLength = 50;
    public const int FullNameMaxLength = 200;
    public const int EmailMaxLength = 320;
    public const int JobTitleMaxLength = 100;

    private Employee()
    {
    }

    private Employee(
        Guid id,
        Guid tenantId,
        string employeeCode,
        string fullName,
        string email,
        string jobTitle,
        Guid departmentId,
        DateOnly joinedDate)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeCode = employeeCode;
        FullName = fullName;
        Email = email;
        JobTitle = jobTitle;
        DepartmentId = departmentId;
        JoinedDate = joinedDate;
        IsActive = true;
    }

    public Guid TenantId { get; private set; }

    public string EmployeeCode { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string JobTitle { get; private set; } = string.Empty;

    public Guid DepartmentId { get; private set; }

    public DateOnly JoinedDate { get; private set; }

    public bool IsActive { get; private set; }

    public static Result<Employee> Create(
        Guid id,
        Guid tenantId,
        string? employeeCode,
        string? fullName,
        string? email,
        string? jobTitle,
        Guid departmentId,
        DateOnly joinedDate)
    {
        if (id == Guid.Empty)
        {
            return EmployeeErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return EmployeeErrors.InvalidTenantId;
        }

        if (!TryNormalizeEmployeeCode(employeeCode, out var normalizedEmployeeCode))
        {
            return EmployeeErrors.InvalidEmployeeCode;
        }

        if (!TryNormalizeFullName(fullName, out var normalizedFullName))
        {
            return EmployeeErrors.InvalidFullName;
        }

        if (!TryNormalizeEmail(email, out var normalizedEmail))
        {
            return EmployeeErrors.InvalidEmail;
        }

        if (!TryNormalizeJobTitle(jobTitle, out var normalizedJobTitle))
        {
            return EmployeeErrors.InvalidJobTitle;
        }

        if (departmentId == Guid.Empty)
        {
            return EmployeeErrors.InvalidDepartmentId;
        }

        if (joinedDate == default)
        {
            return EmployeeErrors.InvalidJoinedDate;
        }

        return new Employee(
            id,
            tenantId,
            normalizedEmployeeCode,
            normalizedFullName,
            normalizedEmail,
            normalizedJobTitle,
            departmentId,
            joinedDate);
    }

    public Result Activate()
    {
        if (IsActive)
        {
            return EmployeeErrors.AlreadyActive;
        }

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return EmployeeErrors.AlreadyInactive;
        }

        IsActive = false;
        return Result.Success();
    }

    private static bool TryNormalizeEmployeeCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length is < EmployeeCodeMinLength or > EmployeeCodeMaxLength)
        {
            return false;
        }

        if (!char.IsAsciiLetterOrDigit(candidate[0]))
        {
            return false;
        }

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
            {
                return false;
            }
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeFullName(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > FullNameMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeEmail(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToLowerInvariant();

        if (candidate.Length > EmailMaxLength || candidate.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var atIndex = candidate.IndexOf('@', StringComparison.Ordinal);

        if (atIndex <= 0 || atIndex != candidate.LastIndexOf('@') || atIndex == candidate.Length - 1)
        {
            return false;
        }

        var domain = candidate[(atIndex + 1)..];

        if (domain.StartsWith('.') || domain.EndsWith('.') || !domain.Contains('.'))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeJobTitle(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > JobTitleMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
