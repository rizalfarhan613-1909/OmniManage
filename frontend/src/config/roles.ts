/**
 * Mirror of the backend contract `SystemRoles` (Platform.Domain.Identity).
 * Roles are used for navigation shaping only; the backend remains the
 * authorization authority (Rules.md §8).
 */
export const SYSTEM_ROLES = {
  systemSuperadmin: 'SYSTEM_SUPERADMIN',
  tenantAdmin: 'TENANT_ADMIN',
  hrManager: 'HR_MANAGER',
  financeManager: 'FINANCE_MANAGER',
  salesRepresentative: 'SALES_REPRESENTATIVE',
  warehouseOperator: 'WAREHOUSE_OPERATOR',
  productionManager: 'PRODUCTION_MANAGER',
  projectManager: 'PROJECT_MANAGER',
  generalStaff: 'GENERAL_STAFF',
} as const;

export type SystemRole = (typeof SYSTEM_ROLES)[keyof typeof SYSTEM_ROLES];
