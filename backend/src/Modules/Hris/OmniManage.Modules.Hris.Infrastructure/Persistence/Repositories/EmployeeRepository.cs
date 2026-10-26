using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Infrastructure.Persistence;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(HrisDbContext dbContext) : IEmployeeRepository
{
    public Task<bool> ExistsAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        dbContext.Employees.AnyAsync(employee => employee.Id == employeeId, cancellationToken);
}
