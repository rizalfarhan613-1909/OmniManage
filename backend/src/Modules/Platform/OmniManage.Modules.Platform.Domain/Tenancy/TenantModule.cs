using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Domain.Tenancy;

public sealed class TenantModule : ITenantEntity
{
    public const int ModuleCodeMaxLength = 50;
    public const int ModuleCodeMinLength = 3;
    public const int ChangeReasonMaxLength = 500;

    private TenantModule()
    {
    }

    private TenantModule(
        Guid tenantId,
        string moduleCode,
        bool isActive,
        DateTimeOffset? activatedAt,
        DateTimeOffset? expiresAt,
        Guid changedBy,
        string changeReason)
    {
        TenantId = tenantId;
        ModuleCode = moduleCode;
        IsActive = isActive;
        ActivatedAt = activatedAt;
        ExpiresAt = expiresAt;
        ChangedBy = changedBy;
        ChangeReason = changeReason;
        Version = 1;
    }

    public Guid TenantId { get; private set; }

    public string ModuleCode { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset? ActivatedAt { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public Guid ChangedBy { get; private set; }

    public string ChangeReason { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public static Result<TenantModule> CreateInactive(
        Guid tenantId,
        string? moduleCode,
        Guid changedBy,
        string? changeReason)
    {
        if (!TryValidate(tenantId, moduleCode, changedBy, changeReason, out var normalizedCode, out var reason, out var error))
        {
            return error;
        }

        return new TenantModule(tenantId, normalizedCode, false, null, null, changedBy, reason);
    }

    public static Result<TenantModule> CreateActive(
        Guid tenantId,
        string? moduleCode,
        Guid changedBy,
        string? changeReason,
        DateTimeOffset activatedAt,
        DateTimeOffset? expiresAt = null)
    {
        if (!TryValidate(tenantId, moduleCode, changedBy, changeReason, out var normalizedCode, out var reason, out var error))
        {
            return error;
        }

        if (activatedAt == default)
        {
            return TenantModuleErrors.InvalidActivationTime;
        }

        if (expiresAt.HasValue && expiresAt.Value <= activatedAt)
        {
            return TenantModuleErrors.InvalidExpiry;
        }

        return new TenantModule(tenantId, normalizedCode, true, activatedAt, expiresAt, changedBy, reason);
    }

    public Result Activate(
        Guid changedBy,
        string? changeReason,
        DateTimeOffset now,
        DateTimeOffset? expiresAt = null)
    {
        if (IsEffectiveAt(now))
        {
            return TenantModuleErrors.AlreadyActive;
        }

        if (!TryValidateChange(changedBy, changeReason, out var reason, out var error))
        {
            return error;
        }

        if (now == default)
        {
            return TenantModuleErrors.InvalidActivationTime;
        }

        if (expiresAt.HasValue && expiresAt.Value <= now)
        {
            return TenantModuleErrors.InvalidExpiry;
        }

        IsActive = true;
        ActivatedAt = now;
        ExpiresAt = expiresAt;
        ChangedBy = changedBy;
        ChangeReason = reason;
        Version++;
        return Result.Success();
    }

    public Result Deactivate(Guid changedBy, string? changeReason)
    {
        if (!IsActive)
        {
            return TenantModuleErrors.AlreadyInactive;
        }

        if (!TryValidateChange(changedBy, changeReason, out var reason, out var error))
        {
            return error;
        }

        IsActive = false;
        ChangedBy = changedBy;
        ChangeReason = reason;
        Version++;
        return Result.Success();
    }

    public Result ChangeExpiry(Guid changedBy, string? changeReason, DateTimeOffset now, DateTimeOffset? expiresAt)
    {
        if (!IsActive)
        {
            return TenantModuleErrors.NotActive;
        }

        if (!TryValidateChange(changedBy, changeReason, out var reason, out var error))
        {
            return error;
        }

        if (expiresAt.HasValue && expiresAt.Value <= now)
        {
            return TenantModuleErrors.InvalidExpiry;
        }

        ExpiresAt = expiresAt;
        ChangedBy = changedBy;
        ChangeReason = reason;
        Version++;
        return Result.Success();
    }

    public bool IsEffectiveAt(DateTimeOffset now) =>
        IsActive && (ExpiresAt is null || ExpiresAt > now);

    private static bool TryValidate(
        Guid tenantId,
        string? moduleCode,
        Guid changedBy,
        string? changeReason,
        out string normalizedCode,
        out string reason,
        out Error error)
    {
        normalizedCode = string.Empty;
        reason = string.Empty;
        error = Error.None;

        if (tenantId == Guid.Empty)
        {
            error = TenantModuleErrors.InvalidTenantId;
            return false;
        }

        if (!TryNormalizeModuleCode(moduleCode, out normalizedCode))
        {
            error = TenantModuleErrors.InvalidModuleCode;
            return false;
        }

        return TryValidateChange(changedBy, changeReason, out reason, out error);
    }

    private static bool TryValidateChange(
        Guid changedBy,
        string? changeReason,
        out string reason,
        out Error error)
    {
        reason = string.Empty;
        error = Error.None;

        if (changedBy == Guid.Empty)
        {
            error = TenantModuleErrors.InvalidChangedBy;
            return false;
        }

        if (string.IsNullOrWhiteSpace(changeReason))
        {
            error = TenantModuleErrors.ChangeReasonRequired;
            return false;
        }

        var candidate = changeReason.Trim();

        if (candidate.Length > ChangeReasonMaxLength)
        {
            error = TenantModuleErrors.ChangeReasonTooLong;
            return false;
        }

        reason = candidate;
        return true;
    }

    private static bool TryNormalizeModuleCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length < ModuleCodeMinLength || candidate.Length > ModuleCodeMaxLength)
        {
            return false;
        }

        if (!char.IsAsciiLetterOrDigit(candidate[0]))
        {
            return false;
        }

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        normalized = candidate;
        return true;
    }
}
