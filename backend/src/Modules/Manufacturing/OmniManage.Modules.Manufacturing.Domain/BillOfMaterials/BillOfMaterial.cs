using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Domain.Errors;

namespace OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

public sealed class BillOfMaterial : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int NotesMaxLength = 1000;
    public const int MaxVersion = 9999;

    private readonly List<BillOfMaterialComponent> _components = [];

    private BillOfMaterial()
    {
    }

    private BillOfMaterial(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        Guid productItemId,
        int version,
        string? notes,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        ProductItemId = productItemId;
        Version = version;
        IsActive = true;
        Notes = notes;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public Guid ProductItemId { get; private set; }

    public int Version { get; private set; }

    public bool IsActive { get; private set; }

    public string? Notes { get; private set; }

    public IReadOnlyList<BillOfMaterialComponent> Components => _components;

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<BillOfMaterial> Create(
        Guid id,
        Guid tenantId,
        string? code,
        string? name,
        Guid productItemId,
        int version,
        string? notes,
        IReadOnlyList<BillOfMaterialComponentInput>? components,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return BillOfMaterialErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return BillOfMaterialErrors.InvalidTenantId;
        }

        if (!TryNormalizeCode(code, out var normalizedCode))
        {
            return BillOfMaterialErrors.InvalidCode;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return BillOfMaterialErrors.InvalidName;
        }

        if (productItemId == Guid.Empty)
        {
            return BillOfMaterialErrors.InvalidProductItemId;
        }

        if (version is < 1 or > MaxVersion)
        {
            return BillOfMaterialErrors.InvalidVersion;
        }

        if (!TryNormalizeNotes(notes, out var normalizedNotes))
        {
            return BillOfMaterialErrors.InvalidNotes;
        }

        if (createdAt == default)
        {
            return BillOfMaterialErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return BillOfMaterialErrors.InvalidCreatedBy;
        }

        if (components is null || components.Count == 0)
        {
            return BillOfMaterialErrors.NoComponents;
        }

        var billOfMaterial = new BillOfMaterial(
            id,
            tenantId,
            normalizedCode,
            normalizedName,
            productItemId,
            version,
            normalizedNotes,
            createdAt,
            createdBy);

        var componentItems = new HashSet<Guid>();

        foreach (var componentInput in components)
        {
            var component = BillOfMaterialComponent.Create(
                componentInput.Id,
                billOfMaterial.Id,
                componentInput.ItemId,
                componentInput.QuantityPerUnit);

            if (component.IsFailure)
            {
                return component.Error;
            }

            if (!componentItems.Add(component.Value.ItemId))
            {
                return BillOfMaterialErrors.DuplicateComponentItem;
            }

            billOfMaterial._components.Add(component.Value);
        }

        return billOfMaterial;
    }

    private static bool TryNormalizeCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length > CodeMaxLength)
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

    private static bool TryNormalizeNotes(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > NotesMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
