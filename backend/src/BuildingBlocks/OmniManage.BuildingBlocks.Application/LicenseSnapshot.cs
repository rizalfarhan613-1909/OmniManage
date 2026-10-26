namespace OmniManage.BuildingBlocks.Application;

public sealed class LicenseSnapshot
{
    private readonly HashSet<string> _activeModuleCodes;

    public LicenseSnapshot(IEnumerable<string> activeModuleCodes)
    {
        ArgumentNullException.ThrowIfNull(activeModuleCodes);

        _activeModuleCodes = new HashSet<string>(activeModuleCodes, StringComparer.Ordinal);
        ActiveModuleCodes = _activeModuleCodes.Order(StringComparer.Ordinal).ToArray();
    }

    public static LicenseSnapshot Empty { get; } = new([]);

    public IReadOnlyList<string> ActiveModuleCodes { get; }

    public bool IsActive(string moduleCode) =>
        !string.IsNullOrWhiteSpace(moduleCode) && _activeModuleCodes.Contains(moduleCode);
}
