using OmniManage.Modules.Hris.Domain.Employees;

namespace OmniManage.Modules.Hris.Application.Dtos;

public sealed record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string JobTitle,
    Guid DepartmentId,
    DateOnly JoinedDate,
    bool IsActive)
{
    internal static EmployeeDto From(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        return new EmployeeDto(
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Email,
            employee.JobTitle,
            employee.DepartmentId,
            employee.JoinedDate,
            employee.IsActive);
    }
}
