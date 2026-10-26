namespace OmniManage.Modules.Hris.Domain.Abstractions;

public interface IEmployeeRepository
{
    Task<bool> ExistsAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
