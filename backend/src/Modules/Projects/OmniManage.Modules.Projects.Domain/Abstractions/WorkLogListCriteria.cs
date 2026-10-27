namespace OmniManage.Modules.Projects.Domain.Abstractions;

public sealed record WorkLogListCriteria(
    int Skip,
    int Take,
    Guid? TaskId,
    Guid? EmployeeId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    IReadOnlyList<WorkLogSortOption> Sort);

public sealed record WorkLogSortOption(WorkLogSortField Field, bool Descending);

public enum WorkLogSortField
{
    LoggedDate = 1,
    HoursSpent = 2,
}
