using System.Reflection;

namespace OmniManage.Modules.MasterData.Presentation;

public static class MasterDataPresentationAssembly
{
    public static Assembly Value { get; } = typeof(MasterDataPresentationAssembly).Assembly;
}
