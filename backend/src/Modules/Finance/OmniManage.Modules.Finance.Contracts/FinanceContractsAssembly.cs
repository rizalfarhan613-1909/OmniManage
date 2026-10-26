using System.Reflection;

namespace OmniManage.Modules.Finance.Contracts;

/// <summary>
/// Identifies the public contract assembly of the Finance module.
/// Other modules may reference only this assembly.
/// </summary>
public static class FinanceContractsAssembly
{
    public static Assembly Value { get; } = typeof(FinanceContractsAssembly).Assembly;
}
