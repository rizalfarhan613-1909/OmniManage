namespace OmniManage.BuildingBlocks.Contracts.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class AllowWithoutModuleAttribute : Attribute
{
}
