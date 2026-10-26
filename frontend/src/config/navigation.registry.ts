import type { LucideIcon } from 'lucide-react';
import {
  Blocks,
  Boxes,
  Briefcase,
  Clock3,
  Factory,
  LayoutDashboard,
  QrCode,
  ShieldCheck,
  ShoppingCart,
  Truck,
  Users,
  Wallet,
} from 'lucide-react';

import { MODULE_CODES, type ModuleCode } from '@/config/module-codes';
import { PERMISSIONS, type PermissionCode } from '@/config/permissions';
import { SYSTEM_ROLES, type SystemRole } from '@/config/roles';
import type { TranslationKey } from '@/lib/i18n';

/**
 * Static navigation registry — the single source of truth for sidebar entries.
 *
 * Every item declares the module that licenses it, plus an optional permission
 * and/or role requirement. `buildSidebar` filters this registry at render time
 * using the session payload (DESIGN.md §3.2). Client-side filtering is a UX
 * convenience only; the backend always re-enforces access.
 */
export interface NavItem {
  id: string;
  labelKey: TranslationKey;
  href?: string;
  icon?: LucideIcon;
  moduleCode: ModuleCode;
  /** Any-of when an array is provided. */
  permission?: PermissionCode | readonly PermissionCode[];
  /** Any-of role gate for administrative views. */
  roles?: readonly SystemRole[];
  children?: NavItem[];
}

/**
 * The 10 core module landings from DESIGN.md §3.5, ordered by the documented
 * business flow: Dashboard → Sales → Inventory → Procurement → Manufacturing
 * → Finance → HR → ESS → Projects → Assets → Administration.
 *
 * Modules with dedicated sub-routes expose them as second-level children. The
 * parent keeps its landing `href`, so `buildSidebar` can drop unlicensed or
 * unauthorized children without hiding the module overview itself.
 */
export const NAVIGATION_REGISTRY: readonly NavItem[] = [
  {
    id: 'dashboard',
    labelKey: 'nav.dashboard',
    href: '/dashboard',
    icon: LayoutDashboard,
    moduleCode: MODULE_CODES.platformCore,
  },
  {
    id: 'sales',
    labelKey: 'nav.sales',
    href: '/sales',
    icon: ShoppingCart,
    moduleCode: MODULE_CODES.salesCore,
    permission: PERMISSIONS.sales.orderRead,
  },
  {
    id: 'inventory',
    labelKey: 'nav.inventory',
    href: '/inventory',
    icon: Boxes,
    moduleCode: MODULE_CODES.invCore,
    permission: PERMISSIONS.inventory.stockRead,
  },
  {
    id: 'procurement',
    labelKey: 'nav.procurement',
    href: '/procurement',
    icon: Truck,
    moduleCode: MODULE_CODES.procCore,
    children: [
      {
        id: 'procurement-suppliers',
        labelKey: 'procurement.suppliers.title',
        href: '/procurement/suppliers',
        moduleCode: MODULE_CODES.procCore,
        permission: PERMISSIONS.procurement.supplierRead,
      },
      {
        id: 'procurement-purchase-orders',
        labelKey: 'procurement.orders.title',
        href: '/procurement/purchase-orders',
        moduleCode: MODULE_CODES.procCore,
        permission: PERMISSIONS.procurement.purchaseOrderRead,
      },
    ],
  },
  {
    id: 'manufacturing',
    labelKey: 'nav.manufacturing',
    href: '/manufacturing',
    icon: Factory,
    moduleCode: MODULE_CODES.mfgCore,
    children: [
      {
        id: 'manufacturing-work-orders',
        labelKey: 'manufacturing.workOrders.title',
        href: '/manufacturing/work-orders',
        moduleCode: MODULE_CODES.mfgCore,
        permission: PERMISSIONS.manufacturing.workOrderRead,
      },
      {
        id: 'manufacturing-bom',
        labelKey: 'manufacturing.bom.title',
        href: '/manufacturing/bom',
        moduleCode: MODULE_CODES.mfgCore,
        permission: PERMISSIONS.manufacturing.billOfMaterialRead,
      },
    ],
  },
  {
    id: 'finance',
    labelKey: 'nav.finance',
    href: '/finance',
    icon: Wallet,
    moduleCode: MODULE_CODES.finGl,
    permission: [PERMISSIONS.finance.journalRead, PERMISSIONS.finance.invoiceRead],
  },
  {
    id: 'hris',
    labelKey: 'nav.hris',
    href: '/hris',
    icon: Users,
    moduleCode: MODULE_CODES.hrisCore,
    children: [
      {
        id: 'hris-attendance',
        labelKey: 'hris.attendance.title',
        href: '/hris/attendance',
        moduleCode: MODULE_CODES.hrisAttendance,
        permission: PERMISSIONS.hris.attendanceRead,
      },
    ],
  },
  {
    id: 'ess',
    labelKey: 'nav.ess',
    href: '/mobile/ess',
    icon: Clock3,
    moduleCode: MODULE_CODES.hrisEss,
  },
  {
    id: 'projects',
    labelKey: 'nav.projects',
    href: '/projects',
    icon: Briefcase,
    moduleCode: MODULE_CODES.projectCore,
    permission: [
      PERMISSIONS.projects.taskRead,
      PERMISSIONS.projects.taskCreate,
      PERMISSIONS.projects.workLogCreate,
    ],
    children: [
      {
        id: 'projects-tasks',
        labelKey: 'projects.tasks.title',
        href: '/projects/tasks',
        moduleCode: MODULE_CODES.projectCore,
        permission: PERMISSIONS.projects.taskRead,
      },
    ],
  },
  {
    id: 'assets',
    labelKey: 'nav.assets',
    href: '/assets',
    icon: QrCode,
    moduleCode: MODULE_CODES.assetCore,
    permission: [
      PERMISSIONS.assets.assetRead,
      PERMISSIONS.assets.assetCreate,
      PERMISSIONS.assets.maintenanceRequest,
    ],
    children: [
      {
        id: 'assets-register',
        labelKey: 'assets.register.title',
        href: '/assets/register',
        moduleCode: MODULE_CODES.assetCore,
        permission: PERMISSIONS.assets.assetRead,
      },
    ],
  },
  {
    id: 'admin',
    labelKey: 'nav.admin',
    icon: Blocks,
    moduleCode: MODULE_CODES.platformCore,
    roles: [SYSTEM_ROLES.tenantAdmin, SYSTEM_ROLES.systemSuperadmin],
    children: [
      {
        id: 'admin-licensing',
        labelKey: 'nav.admin.licensing',
        href: '/admin/licensing',
        icon: Blocks,
        moduleCode: MODULE_CODES.platformCore,
        roles: [SYSTEM_ROLES.tenantAdmin, SYSTEM_ROLES.systemSuperadmin],
      },
      {
        id: 'admin-security',
        labelKey: 'nav.admin.security',
        href: '/admin/security',
        icon: ShieldCheck,
        moduleCode: MODULE_CODES.platformCore,
        roles: [SYSTEM_ROLES.tenantAdmin, SYSTEM_ROLES.systemSuperadmin],
      },
    ],
  },
];
