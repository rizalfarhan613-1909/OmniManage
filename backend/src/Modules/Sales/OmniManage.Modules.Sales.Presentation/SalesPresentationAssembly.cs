using System.Reflection;

namespace OmniManage.Modules.Sales.Presentation;

public static class SalesPresentationAssembly
{
    public static Assembly Value { get; } = typeof(SalesPresentationAssembly).Assembly;
}
