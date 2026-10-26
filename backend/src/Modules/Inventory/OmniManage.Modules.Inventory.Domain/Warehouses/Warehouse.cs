using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Domain.Errors;

namespace OmniManage.Modules.Inventory.Domain.Warehouses;

public sealed class Warehouse : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int CodeMinLength = 2;
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int AddressMaxLength = 500;

    private Warehouse()
    {
    }

    private Warehouse(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? address,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        Address = address;
        IsActive = true;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Address { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public Result Activate(DateTimeOffset updatedAt, Guid updatedBy) =>
        SetActive(isActive: true, updatedAt, updatedBy);

    public Result Deactivate(DateTimeOffset updatedAt, Guid updatedBy) =>
        SetActive(isActive: false, updatedAt, updatedBy);

    public static Result<Warehouse> Create(
        Guid id,
        Guid tenantId,
        string? code,
        string? name,
        string? address,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return WarehouseErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return WarehouseErrors.InvalidTenantId;
        }

        if (!TryNormalizeCode(code, out var normalizedCode))
        {
            return WarehouseErrors.InvalidCode;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return WarehouseErrors.InvalidName;
        }

        if (!TryNormalizeAddress(address, out var normalizedAddress))
        {
            return WarehouseErrors.InvalidAddress;
        }

        if (createdAt == default)
        {
            return WarehouseErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return WarehouseErrors.InvalidCreatedBy;
        }

        return new Warehouse(
            id,
            tenantId,
            normalizedCode,
            normalizedName,
            normalizedAddress,
            createdAt,
            createdBy);
    }

    private Result SetActive(bool isActive, DateTimeOffset updatedAt, Guid updatedBy)
    {
        if (updatedAt == default)
        {
            return WarehouseErrors.InvalidUpdatedAt;
        }

        if (updatedBy == Guid.Empty)
        {
            return WarehouseErrors.InvalidUpdatedBy;
        }

        IsActive = isActive;
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;

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

    private static bool TryNormalizeAddress(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > AddressMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
