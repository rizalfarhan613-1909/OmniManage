using System.Reflection;

namespace OmniManage.Modules.Hris.Presentation;

public static class HrisPresentationAssembly
{
    public static Assembly Value { get; } = typeof(HrisPresentationAssembly).Assembly;
}
