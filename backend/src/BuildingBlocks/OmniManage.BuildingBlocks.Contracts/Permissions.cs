using System.Collections.Frozen;

namespace OmniManage.BuildingBlocks.Contracts;

public static class Permissions
{
    /// <summary>
    /// The authoritative permission catalogue. Seeding and architecture tests
    /// consume this set so a permission can never exist only as a string literal.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = new[]
    {
        MasterData.BusinessPartnerRead,
        MasterData.BusinessPartnerCreate,
        MasterData.ItemRead,
        MasterData.ItemCreate,
        Sales.OrderRead,
        Sales.OrderCreate,
        Sales.OrderConfirm,
        Procurement.SupplierRead,
        Procurement.PurchaseOrderRead,
        Procurement.PurchaseOrderCreate,
        Manufacturing.WorkOrderRead,
        Manufacturing.WorkOrderCreate,
        Manufacturing.BillOfMaterialRead,
        Finance.InvoiceRead,
        Finance.InvoiceApprove,
        Finance.JournalRead,
        Finance.PaymentApprove,
        Crm.LeadRead,
        Crm.CustomerReadAll,
        Hris.EmployeeViewSensitive,
        Hris.AttendanceClockIn,
        Hris.AttendanceRead,
        Hris.PayrollApprove,
        Inventory.WarehouseRead,
        Inventory.WarehouseCreate,
        Inventory.StockRead,
        Inventory.StockAdjust,
        Projects.TaskCreate,
        Projects.TaskRead,
        Projects.WorkLogCreate,
        Assets.AssetCreate,
        Assets.AssetRead,
        Assets.MaintenanceRequest,
        Platform.TenantRead,
        Platform.TenantManage,
        Platform.AuditRead,
        Platform.SessionRead,
        Platform.SessionTerminate,
    }.ToFrozenSet(StringComparer.Ordinal);

    public static class MasterData
    {
        public const string BusinessPartnerRead = "masterdata.business_partner.read";
        public const string BusinessPartnerCreate = "masterdata.business_partner.create";
        public const string ItemRead = "masterdata.item.read";
        public const string ItemCreate = "masterdata.item.create";
    }

    public static class Sales
    {
        public const string OrderRead = "sales.order.read";
        public const string OrderCreate = "sales.order.create";
        public const string OrderConfirm = "sales.order.confirm";
    }

    public static class Procurement
    {
        public const string SupplierRead = "procurement.supplier.read";
        public const string PurchaseOrderRead = "procurement.purchase_order.read";
        public const string PurchaseOrderCreate = "procurement.purchase_order.create";
    }

    public static class Manufacturing
    {
        public const string WorkOrderRead = "manufacturing.work_order.read";
        public const string WorkOrderCreate = "manufacturing.work_order.create";
        public const string BillOfMaterialRead = "manufacturing.bill_of_material.read";
    }

    public static class Finance
    {
        public const string InvoiceRead = "finance.invoice.read";
        public const string InvoiceApprove = "finance.invoice.approve";
        public const string JournalRead = "finance.journal.read";
        public const string PaymentApprove = "finance.payment.approve";
    }

    public static class Crm
    {
        public const string LeadRead = "crm.lead.read";
        public const string CustomerReadAll = "crm.customer.read_all";
    }

    public static class Hris
    {
        public const string EmployeeViewSensitive = "hris.employee.view_sensitive";
        public const string AttendanceClockIn = "hris.attendance.clock_in";
        public const string AttendanceRead = "hris.attendance.read";
        public const string PayrollApprove = "hris.payroll.approve";
    }

    public static class Inventory
    {
        public const string WarehouseRead = "inventory.warehouse.read";
        public const string WarehouseCreate = "inventory.warehouse.create";
        public const string StockRead = "inventory.stock.read";
        public const string StockAdjust = "inventory.stock.adjust";
    }

    public static class Projects
    {
        public const string TaskCreate = "projects.task.create";
        public const string TaskRead = "projects.task.read";
        public const string WorkLogCreate = "projects.worklog.create";
    }

    public static class Assets
    {
        public const string AssetCreate = "assets.asset.create";
        public const string AssetRead = "assets.asset.read";
        public const string MaintenanceRequest = "assets.maintenance.request";
    }

    public static class Platform
    {
        public const string TenantRead = "platform.tenant.read";
        public const string TenantManage = "platform.tenant.manage";
        public const string AuditRead = "platform.audit.read";
        public const string SessionRead = "platform.session.read";
        public const string SessionTerminate = "platform.session.terminate";
    }
}