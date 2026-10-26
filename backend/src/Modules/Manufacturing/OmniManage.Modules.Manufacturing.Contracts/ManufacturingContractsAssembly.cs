using System.Reflection;

namespace OmniManage.Modules.Manufacturing.Contracts;

/// <summary>
/// Identifies the public contract assembly of the Manufacturing module.
/// Other modules may reference only this assembly.
/// </summary>
public static class ManufacturingContractsAssembly
{
    public static Assembly Value { get; } = typeof(ManufacturingContractsAssembly).Assembly;
}
