using System.Reflection;

namespace OmniManage.Modules.Procurement.Contracts;

/// <summary>
/// Identifies the public contract assembly of the Procurement module.
/// Other modules may reference only this assembly.
/// </summary>
public static class ProcurementContractsAssembly
{
    public static Assembly Value { get; } = typeof(ProcurementContractsAssembly).Assembly;
}
