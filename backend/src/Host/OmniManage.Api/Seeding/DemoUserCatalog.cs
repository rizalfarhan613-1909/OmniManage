using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Login fixtures for local development. Every account shares the configured
/// demo password so the UI can be exercised from each role's perspective.
/// </summary>
internal static class DemoUserCatalog
{
    public static IReadOnlyList<DemoUserAccount> Users { get; } =
    [
        new("budi.santoso@acme.com", SystemRoles.HrManager),
        new("dewi.anggraini@acme.com", SystemRoles.HrManager),
        new("rudi.hartono@acme.com", SystemRoles.FinanceManager),
        new("siti.nurhaliza@acme.com", SystemRoles.FinanceManager),
        new("andi.wijaya@acme.com", SystemRoles.SalesRepresentative),
        new("maya.lestari@acme.com", SystemRoles.SalesRepresentative),
        new("joko.prasetyo@acme.com", SystemRoles.WarehouseOperator),
        new("agus.salim@acme.com", SystemRoles.WarehouseOperator),
        new("rina.kartika@acme.com", SystemRoles.ProductionManager),
        new("hendra.gunawan@acme.com", SystemRoles.ProjectManager),
        new("putri.mahardika@acme.com", SystemRoles.GeneralStaff),
        new("fajar.nugroho@acme.com", SystemRoles.GeneralStaff),
        new("lina.marlina@acme.com", SystemRoles.GeneralStaff),
    ];

    public static IReadOnlyDictionary<string, string[]> PermissionsByRole { get; } =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [SystemRoles.HrManager] =
            [
                Permissions.Hris.EmployeeViewSensitive,
                Permissions.Hris.AttendanceClockIn,
                Permissions.Hris.AttendanceRead,
                Permissions.Hris.PayrollApprove,
                Permissions.MasterData.ItemRead,
            ],
            [SystemRoles.FinanceManager] =
            [
                Permissions.Finance.InvoiceRead,
                Permissions.Finance.InvoiceApprove,
                Permissions.Finance.JournalRead,
                Permissions.Finance.PaymentApprove,
                Permissions.MasterData.BusinessPartnerRead,
                Permissions.MasterData.ItemRead,
            ],
            [SystemRoles.SalesRepresentative] =
            [
                Permissions.Sales.OrderRead,
                Permissions.Sales.OrderCreate,
                Permissions.Sales.OrderConfirm,
                Permissions.Crm.LeadRead,
                Permissions.Crm.CustomerReadAll,
                Permissions.MasterData.BusinessPartnerRead,
                Permissions.MasterData.ItemRead,
            ],
            [SystemRoles.WarehouseOperator] =
            [
                Permissions.Inventory.WarehouseRead,
                Permissions.Inventory.WarehouseCreate,
                Permissions.Inventory.StockRead,
                Permissions.Inventory.StockAdjust,
                Permissions.Procurement.PurchaseOrderRead,
                Permissions.Assets.AssetRead,
                Permissions.MasterData.ItemRead,
            ],
            [SystemRoles.ProductionManager] =
            [
                Permissions.Manufacturing.WorkOrderRead,
                Permissions.Manufacturing.WorkOrderCreate,
                Permissions.Manufacturing.BillOfMaterialRead,
                Permissions.Procurement.SupplierRead,
                Permissions.Procurement.PurchaseOrderRead,
                Permissions.Procurement.PurchaseOrderCreate,
                Permissions.Projects.TaskCreate,
                Permissions.Projects.TaskRead,
                Permissions.Projects.WorkLogCreate,
                Permissions.Inventory.StockRead,
                Permissions.Assets.AssetCreate,
                Permissions.Assets.AssetRead,
                Permissions.Assets.MaintenanceRequest,
            ],
            [SystemRoles.ProjectManager] =
            [
                Permissions.Projects.TaskCreate,
                Permissions.Projects.TaskRead,
                Permissions.Projects.WorkLogCreate,
                Permissions.Hris.EmployeeViewSensitive,
                Permissions.Hris.AttendanceRead,
                Permissions.Assets.AssetRead,
                Permissions.Assets.MaintenanceRequest,
                Permissions.MasterData.BusinessPartnerRead,
            ],
            [SystemRoles.GeneralStaff] =
            [
                Permissions.Hris.AttendanceClockIn,
                Permissions.Projects.TaskRead,
                Permissions.Projects.WorkLogCreate,
                Permissions.MasterData.ItemRead,
            ],
        };
}

internal sealed record DemoUserAccount(string Email, string RoleName);
