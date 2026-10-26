using System.Reflection;

namespace OmniManage.Modules.Hris.Contracts;

/// <summary>
/// Identifies the public contract assembly of the HRIS module.
/// Other modules may reference only this assembly.
/// </summary>
public static class HrisContractsAssembly
{
    public static Assembly Value { get; } = typeof(HrisContractsAssembly).Assembly;
}
