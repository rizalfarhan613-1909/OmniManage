namespace OmniManage.Modules.Hris.Domain.Abstractions;

public sealed record EmployeeListCriteria(
    int Skip,
    int Take,
    string? Search,
    Guid? DepartmentId,
    bool? IsActive,
    IReadOnlyList<EmployeeSortOption> Sort);

public sealed record EmployeeSortOption(EmployeeSortField Field, bool Descending);

public enum EmployeeSortField
{
    EmployeeCode = 1,
    FullName = 2,
    JoinedDate = 3,
}
