using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Domain.Tenancy;

public sealed class Tenant : AggregateRoot<Guid>
{
    public const int CodeMaxLength = 50;
    public const int CodeMinLength = 3;
    public const int NameMaxLength = 200;
    public const int PlanCodeMaxLength = 50;
    public const int PlanCodeMinLength = 2;

    private Tenant()
    {
    }

    private Tenant(Guid id, string code, string name, string planCode, DateTimeOffset createdAt)
        : base(id)
    {
        Code = code;
        Name = name;
        Status = TenantStatus.Active;
        PlanCode = planCode;
        CreatedAt = createdAt;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public TenantStatus Status { get; private set; }

    public string PlanCode { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<Tenant> Create(
        Guid id,
        string? code,
        string? name,
        string? planCode,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            return TenantErrors.InvalidId;
        }

        if (!TryNormalizeCode(code, CodeMinLength, CodeMaxLength, out var normalizedCode))
        {
            return TenantErrors.InvalidCode;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return TenantErrors.InvalidName;
        }

        if (!TryNormalizeCode(planCode, PlanCodeMinLength, PlanCodeMaxLength, out var normalizedPlanCode))
        {
            return TenantErrors.InvalidPlanCode;
        }

        if (createdAt == default)
        {
            return TenantErrors.InvalidCreatedAt;
        }

        return new Tenant(id, normalizedCode, normalizedName, normalizedPlanCode, createdAt);
    }

    public Result Suspend()
    {
        if (Status != TenantStatus.Active)
        {
            return TenantErrors.InvalidStatusTransition(Status, "suspend");
        }

        Status = TenantStatus.Suspended;
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (Status != TenantStatus.Suspended)
        {
            return TenantErrors.InvalidStatusTransition(Status, "reactivate");
        }

        Status = TenantStatus.Active;
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == TenantStatus.Archived)
        {
            return TenantErrors.InvalidStatusTransition(Status, "archive");
        }

        Status = TenantStatus.Archived;
        return Result.Success();
    }

    private static bool TryNormalizeCode(string? value, int minLength, int maxLength, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length < minLength || candidate.Length > maxLength)
        {
            return false;
        }

        if (!char.IsAsciiLetterOrDigit(candidate[0]))
        {
            return false;
        }

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_'))
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
}
