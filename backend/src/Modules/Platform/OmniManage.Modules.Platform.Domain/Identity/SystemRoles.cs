using System.Collections.Frozen;

namespace OmniManage.Modules.Platform.Domain.Identity;

public static class SystemRoles
{
    public const string TenantAdmin = "TENANT_ADMIN";
    public const string HrManager = "HR_MANAGER";
    public const string FinanceManager = "FINANCE_MANAGER";
    public const string SalesRepresentative = "SALES_REPRESENTATIVE";
    public const string WarehouseOperator = "WAREHOUSE_OPERATOR";
    public const string ProductionManager = "PRODUCTION_MANAGER";
    public const string ProjectManager = "PROJECT_MANAGER";
    public const string GeneralStaff = "GENERAL_STAFF";

    public static IReadOnlySet<string> All { get; } = new[]
    {
        TenantAdmin,
        HrManager,
        FinanceManager,
        SalesRepresentative,
        WarehouseOperator,
        ProductionManager,
        ProjectManager,
        GeneralStaff,
    }.ToFrozenSet(StringComparer.Ordinal);
}
