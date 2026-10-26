using System.Reflection;

namespace OmniManage.Modules.MasterData.Contracts;

/// <summary>
/// Identifies the public contract assembly of the MasterData module.
/// Other modules may reference only this assembly.
/// </summary>
public static class MasterDataContractsAssembly
{
    public static Assembly Value { get; } = typeof(MasterDataContractsAssembly).Assembly;
}
