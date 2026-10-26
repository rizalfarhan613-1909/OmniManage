using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Domain.Identity;

public sealed class Permission : Entity<string>
{
    public const int CodeMaxLength = 150;
    public const int MaxSegmentLength = 50;

    private Permission()
    {
    }

    private Permission(string code)
        : base(code)
    {
    }

    public string Code => Id;

    public static Result<Permission> Create(string? code)
    {
        if (!TryNormalizeCode(code, out var normalizedCode))
        {
            return PermissionErrors.InvalidCode;
        }

        return new Permission(normalizedCode);
    }

    private static bool TryNormalizeCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToLowerInvariant();

        if (candidate.Length > CodeMaxLength)
        {
            return false;
        }

        var segments = candidate.Split('.');

        if (segments.Length < 3)
        {
            return false;
        }

        foreach (var segment in segments)
        {
            if (!IsValidSegment(segment))
            {
                return false;
            }
        }

        normalized = candidate;
        return true;
    }

    private static bool IsValidSegment(string segment)
    {
        if (segment.Length is < 1 or > MaxSegmentLength || !char.IsAsciiLetterLower(segment[0]))
        {
            return false;
        }

        foreach (var character in segment)
        {
            if (!char.IsAsciiLetterLower(character) && !char.IsAsciiDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }
}
