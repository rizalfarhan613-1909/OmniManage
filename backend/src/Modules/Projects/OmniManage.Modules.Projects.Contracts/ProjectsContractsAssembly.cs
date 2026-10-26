using System.Reflection;

namespace OmniManage.Modules.Projects.Contracts;

/// <summary>
/// Identifies the public contract assembly of the Projects module.
/// Other modules may reference only this assembly.
/// </summary>
public static class ProjectsContractsAssembly
{
    public static Assembly Value { get; } = typeof(ProjectsContractsAssembly).Assembly;
}
