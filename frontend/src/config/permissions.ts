/**
 * Mirror of the backend contract `OmniManage.BuildingBlocks.Contracts.Permissions`.
 * Permission codes MUST NOT be written as string literals in UI code (Rules.md §16.10).
 */
export const PERMISSIONS = {
  masterData: {
    businessPartnerRead: 'masterdata.business_partner.read',
    businessPartnerCreate: 'masterdata.business_partner.create',
    itemRead: 'masterdata.item.read',
    itemCreate: 'masterdata.item.create',
  },
  sales: {
    orderRead: 'sales.order.read',
    orderCreate: 'sales.order.create',
    orderConfirm: 'sales.order.confirm',
  },
  procurement: {
    supplierRead: 'procurement.supplier.read',
    purchaseOrderRead: 'procurement.purchase_order.read',
    purchaseOrderCreate: 'procurement.purchase_order.create',
  },
  manufacturing: {
    workOrderRead: 'manufacturing.work_order.read',
    workOrderCreate: 'manufacturing.work_order.create',
    billOfMaterialRead: 'manufacturing.bill_of_material.read',
  },
  finance: {
    invoiceRead: 'finance.invoice.read',
    invoiceApprove: 'finance.invoice.approve',
    journalRead: 'finance.journal.read',
    paymentApprove: 'finance.payment.approve',
  },
  crm: {
    leadRead: 'crm.lead.read',
    customerReadAll: 'crm.customer.read_all',
  },
  hris: {
    employeeViewSensitive: 'hris.employee.view_sensitive',
    attendanceRead: 'hris.attendance.read',
    attendanceClockIn: 'hris.attendance.clock_in',
    payrollApprove: 'hris.payroll.approve',
  },
  projects: {
    taskRead: 'projects.task.read',
    taskCreate: 'projects.task.create',
    workLogCreate: 'projects.worklog.create',
  },
  assets: {
    assetRead: 'assets.asset.read',
    assetCreate: 'assets.asset.create',
    maintenanceRequest: 'assets.maintenance.request',
  },
  inventory: {
    warehouseRead: 'inventory.warehouse.read',
    warehouseCreate: 'inventory.warehouse.create',
    stockRead: 'inventory.stock.read',
    stockAdjust: 'inventory.stock.adjust',
  },
} as const;

type PermissionGroups = typeof PERMISSIONS;

export type PermissionCode = {
  [Group in keyof PermissionGroups]: PermissionGroups[Group][keyof PermissionGroups[Group]];
}[keyof PermissionGroups];
