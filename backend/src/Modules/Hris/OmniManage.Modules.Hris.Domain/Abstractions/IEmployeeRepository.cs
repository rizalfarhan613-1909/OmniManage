using OmniManage.Modules.Hris.Domain.Employees;

namespace OmniManage.Modules.Hris.Domain.Abstractions;

public interface IEmployeeRepository
{
    Task<bool> ExistsAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Employee> Employees, long TotalItems)> ListAsync(
        EmployeeListCriteria criteria,
        CancellationToken cancellationToken = default);
}
