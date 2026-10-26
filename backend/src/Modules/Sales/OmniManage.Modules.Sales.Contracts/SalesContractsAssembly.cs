using System.Reflection;

namespace OmniManage.Modules.Sales.Contracts;

/// <summary>
/// Identifies the public contract assembly of the Sales module.
/// Other modules may reference only this assembly.
/// </summary>
public static class SalesContractsAssembly
{
    public static Assembly Value { get; } = typeof(SalesContractsAssembly).Assembly;
}
