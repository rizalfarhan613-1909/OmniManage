using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Projects.Domain.Errors;

namespace OmniManage.Modules.Projects.Domain.Projects;

public sealed class Project : AggregateRoot<Guid>, ITenantEntity
{
    public const int CodeMinLength = 2;
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 1000;

    private Project()
    {
    }

    private Project(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? description,
        Guid customerId,
        DateOnly startDate,
        DateOnly? endDate)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        Description = description;
        CustomerId = customerId;
        StartDate = startDate;
        EndDate = endDate;
        Status = ProjectStatus.Planned;
    }

    public Guid TenantId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid CustomerId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public ProjectStatus Status { get; private set; }

    public static Result<Project> Create(
        Guid id,
        Guid tenantId,
        string? code,
        string? name,
        string? description,
        Guid customerId,
        DateOnly startDate,
        DateOnly? endDate)
    {
        if (id == Guid.Empty)
        {
            return ProjectErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return ProjectErrors.InvalidTenantId;
        }

        if (!TryNormalizeCode(code, out var normalizedCode))
        {
            return ProjectErrors.InvalidCode;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return ProjectErrors.InvalidName;
        }

        if (!TryNormalizeDescription(description, out var normalizedDescription))
        {
            return ProjectErrors.InvalidDescription;
        }

        if (customerId == Guid.Empty)
        {
            return ProjectErrors.InvalidCustomerId;
        }

        if (startDate == default)
        {
            return ProjectErrors.InvalidStartDate;
        }

        if (endDate.HasValue && endDate.Value < startDate)
        {
            return ProjectErrors.InvalidEndDate;
        }

        return new Project(
            id,
            tenantId,
            normalizedCode,
            normalizedName,
            normalizedDescription,
            customerId,
            startDate,
            endDate);
    }

    public Result Activate()
    {
        if (Status is not (ProjectStatus.Planned or ProjectStatus.OnHold))
        {
            return ProjectErrors.InvalidStatusTransition;
        }

        Status = ProjectStatus.Active;

        return Result.Success();
    }

    public Result PutOnHold()
    {
        if (Status != ProjectStatus.Active)
        {
            return ProjectErrors.InvalidStatusTransition;
        }

        Status = ProjectStatus.OnHold;

        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != ProjectStatus.Active)
        {
            return ProjectErrors.InvalidStatusTransition;
        }

        Status = ProjectStatus.Completed;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
        {
            return ProjectErrors.InvalidStatusTransition;
        }

        Status = ProjectStatus.Cancelled;

        return Result.Success();
    }

    private static bool TryNormalizeCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length is < CodeMinLength or > CodeMaxLength)
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

    private static bool TryNormalizeName(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > NameMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeDescription(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > DescriptionMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
