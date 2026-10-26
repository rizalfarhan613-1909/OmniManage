using System.Reflection;

namespace OmniManage.Modules.Inventory.Contracts;

/// <summary>
/// Identifies the public contract assembly of the Inventory module.
/// Other modules may reference only this assembly.
/// </summary>
public static class InventoryContractsAssembly
{
    public static Assembly Value { get; } = typeof(InventoryContractsAssembly).Assembly;
}
