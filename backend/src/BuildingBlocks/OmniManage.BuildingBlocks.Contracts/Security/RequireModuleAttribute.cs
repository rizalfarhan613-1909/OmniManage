namespace OmniManage.BuildingBlocks.Contracts.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireModuleAttribute : Attribute
{
    public RequireModuleAttribute(string moduleCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleCode);

        ModuleCode = moduleCode;
    }

    public string ModuleCode { get; }
}
