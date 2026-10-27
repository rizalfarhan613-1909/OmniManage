using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Platform.Application.Auth;

public static class PermissionModuleResolver
{
    // Unknown permission codes resolve to no module and are therefore not granted (fail closed).
    public static bool IsEnabled(string permissionCode, LicenseSnapshot licenses)
    {
        ArgumentNullException.ThrowIfNull(licenses);

        return TryResolveModuleCode(permissionCode, out var moduleCode)
            && licenses.IsActive(moduleCode);
    }

    public static bool TryResolveModuleCode(string? permissionCode, out string moduleCode)
    {
        moduleCode = string.Empty;

        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        moduleCode = permissionCode switch
        {
            Permissions.MasterData.BusinessPartnerRead
                or Permissions.MasterData.BusinessPartnerCreate
                or Permissions.MasterData.ItemRead
                or Permissions.MasterData.ItemCreate => ModuleCodes.CoreMasterData,
            Permissions.Sales.OrderRead
                or Permissions.Sales.OrderCreate
                or Permissions.Sales.OrderConfirm => ModuleCodes.SalesCore,
            Permissions.Procurement.SupplierRead
                or Permissions.Procurement.PurchaseOrderRead
                or Permissions.Procurement.PurchaseOrderCreate => ModuleCodes.ProcCore,
            Permissions.Manufacturing.WorkOrderRead
                or Permissions.Manufacturing.WorkOrderCreate
                or Permissions.Manufacturing.BillOfMaterialRead => ModuleCodes.MfgCore,
            Permissions.Finance.InvoiceRead
                or Permissions.Finance.InvoiceApprove => ModuleCodes.FinAr,
            Permissions.Finance.JournalRead => ModuleCodes.FinGl,
            Permissions.Finance.PaymentApprove => ModuleCodes.FinAp,
            Permissions.Crm.LeadRead
                or Permissions.Crm.CustomerReadAll => ModuleCodes.CrmCore,
            Permissions.Hris.EmployeeViewSensitive => ModuleCodes.HrisCore,
            Permissions.Hris.AttendanceClockIn
                or Permissions.Hris.AttendanceRead => ModuleCodes.HrisAttendance,
            Permissions.Hris.PayrollApprove => ModuleCodes.HrisPayroll,
            Permissions.Projects.TaskCreate
                or Permissions.Projects.TaskRead
                or Permissions.Projects.WorkLogCreate => ModuleCodes.ProjectCore,
            Permissions.Assets.AssetCreate
                or Permissions.Assets.AssetRead
                or Permissions.Assets.MaintenanceRequest => ModuleCodes.AssetCore,
            Permissions.Inventory.WarehouseRead
                or Permissions.Inventory.WarehouseCreate
                or Permissions.Inventory.StockRead
                or Permissions.Inventory.StockAdjust => ModuleCodes.InvCore,
            Permissions.Platform.TenantRead
                or Permissions.Platform.TenantManage
                or Permissions.Platform.AuditRead
                or Permissions.Platform.SessionRead
                or Permissions.Platform.SessionTerminate => ModuleCodes.PlatformCore,
            _ => string.Empty,
        };

        return moduleCode.Length > 0;
    }
}
