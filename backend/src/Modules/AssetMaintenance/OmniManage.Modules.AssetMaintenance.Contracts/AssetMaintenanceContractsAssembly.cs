using System.Reflection;

namespace OmniManage.Modules.AssetMaintenance.Contracts;

/// <summary>
/// Identifies the public contract assembly of the AssetMaintenance module.
/// Other modules may reference only this assembly.
/// </summary>
public static class AssetMaintenanceContractsAssembly
{
    public static Assembly Value { get; } = typeof(AssetMaintenanceContractsAssembly).Assembly;
}
