using System.Reflection;

namespace OmniManage.Modules.Platform.Presentation;

public static class PlatformPresentationAssembly
{
    public static Assembly Value { get; } = typeof(PlatformPresentationAssembly).Assembly;
}
