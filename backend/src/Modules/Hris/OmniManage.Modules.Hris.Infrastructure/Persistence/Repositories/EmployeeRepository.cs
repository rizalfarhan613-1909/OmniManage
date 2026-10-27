using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Domain.Employees;
using OmniManage.Modules.Hris.Infrastructure.Persistence;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(HrisDbContext dbContext) : IEmployeeRepository
{
    public Task<bool> ExistsAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        dbContext.Employees.AnyAsync(employee => employee.Id == employeeId, cancellationToken);

    public async Task<(IReadOnlyList<Employee> Employees, long TotalItems)> ListAsync(
        EmployeeListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Employee> query = dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(employee =>
                employee.EmployeeCode.ToLower().Contains(term)
                || employee.FullName.ToLower().Contains(term)
                || employee.Email.ToLower().Contains(term));
        }

        if (criteria.DepartmentId is { } departmentId)
        {
            query = query.Where(employee => employee.DepartmentId == departmentId);
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(employee => employee.IsActive == isActive);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var employees = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (employees, totalItems);
    }

    private static IQueryable<Employee> ApplySort(
        IQueryable<Employee> query,
        EmployeeListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new EmployeeSortOption(EmployeeSortField.EmployeeCode, false)
            : criteria.Sort[0];

        IOrderedQueryable<Employee> ordered = option.Field switch
        {
            EmployeeSortField.FullName => option.Descending
                ? query.OrderByDescending(employee => employee.FullName)
                : query.OrderBy(employee => employee.FullName),
            EmployeeSortField.JoinedDate => option.Descending
                ? query.OrderByDescending(employee => employee.JoinedDate)
                : query.OrderBy(employee => employee.JoinedDate),
            EmployeeSortField.EmployeeCode => option.Descending
                ? query.OrderByDescending(employee => employee.EmployeeCode)
                : query.OrderBy(employee => employee.EmployeeCode),
            _ => query.OrderBy(employee => employee.EmployeeCode),
        };

        return ordered.ThenBy(employee => employee.Id);
    }
}
