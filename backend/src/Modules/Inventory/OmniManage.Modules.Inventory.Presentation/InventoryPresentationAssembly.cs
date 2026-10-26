using System.Reflection;

namespace OmniManage.Modules.Inventory.Presentation;

public static class InventoryPresentationAssembly
{
    public static Assembly Value { get; } = typeof(InventoryPresentationAssembly).Assembly;
}
