using System.Reflection;

namespace OmniManage.Modules.Finance.Presentation;

public static class FinancePresentationAssembly
{
    public static Assembly Value { get; } = typeof(FinancePresentationAssembly).Assembly;
}
