import { describe, expect, it } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { SYSTEM_ROLES } from '@/config/roles';
import { NAVIGATION_REGISTRY, type NavItem } from '@/config/navigation.registry';
import { buildSidebar, isNavItemActive } from '@/lib/navigation/buildSidebar';

const registry: readonly NavItem[] = [
  {
    id: 'dashboard',
    labelKey: 'nav.dashboard',
    href: '/dashboard',
    moduleCode: MODULE_CODES.platformCore,
  },
  {
    id: 'sales',
    labelKey: 'nav.sales',
    href: '/sales',
    moduleCode: MODULE_CODES.salesCore,
    permission: PERMISSIONS.sales.orderRead,
  },
  {
    id: 'finance',
    labelKey: 'nav.finance',
    href: '/finance',
    moduleCode: MODULE_CODES.finGl,
    permission: [PERMISSIONS.finance.invoiceRead, PERMISSIONS.finance.journalRead],
  },
  {
    id: 'admin',
    labelKey: 'nav.admin',
    moduleCode: MODULE_CODES.platformCore,
    roles: [SYSTEM_ROLES.tenantAdmin],
    children: [
      {
        id: 'admin-licensing',
        labelKey: 'nav.admin.licensing',
        href: '/admin/licensing',
        moduleCode: MODULE_CODES.platformCore,
        roles: [SYSTEM_ROLES.tenantAdmin],
      },
      {
        id: 'admin-security',
        labelKey: 'nav.admin.security',
        href: '/admin/security',
        moduleCode: MODULE_CODES.platformCore,
        roles: [SYSTEM_ROLES.hrManager],
      },
    ],
  },
];

describe('buildSidebar', () => {
  it('hides items whose required module is not active', () => {
    const result = buildSidebar(
      registry,
      [MODULE_CODES.platformCore],
      [PERMISSIONS.sales.orderRead],
    );

    expect(result.map((item) => item.id)).toEqual(['dashboard']);
  });

  it('shows an item when its module is active and its permission is granted', () => {
    const result = buildSidebar(
      registry,
      [MODULE_CODES.platformCore, MODULE_CODES.salesCore],
      [PERMISSIONS.sales.orderRead],
    );

    expect(result.map((item) => item.id)).toEqual(['dashboard', 'sales']);
  });

  it('hides an item when its module is active but the permission is missing', () => {
    const result = buildSidebar(registry, [MODULE_CODES.platformCore, MODULE_CODES.salesCore], []);

    expect(result.map((item) => item.id)).toEqual(['dashboard']);
  });

  it('grants an item when any one of its permissions is held', () => {
    const result = buildSidebar(registry, [MODULE_CODES.finGl], [PERMISSIONS.finance.journalRead]);

    expect(result.map((item) => item.id)).toEqual(['finance']);
  });

  it('hides role-gated groups from users without the role', () => {
    const result = buildSidebar(
      registry,
      [MODULE_CODES.platformCore],
      [],
      [SYSTEM_ROLES.generalStaff],
    );

    expect(result.map((item) => item.id)).toEqual(['dashboard']);
  });

  it('filters children independently and drops emptied parents', () => {
    const result = buildSidebar(
      registry,
      [MODULE_CODES.platformCore],
      [],
      [SYSTEM_ROLES.tenantAdmin],
    );

    expect(result.map((item) => item.id)).toEqual(['dashboard', 'admin']);
    expect(result[1]?.children?.map((item) => item.id)).toEqual(['admin-licensing']);
  });

  it('drops a parent that has no href when every child is filtered out', () => {
    const result = buildSidebar(registry, [MODULE_CODES.platformCore], [], []);

    expect(result.map((item) => item.id)).toEqual(['dashboard']);
  });

  it('accepts Sets, matching the session-payload helper signature', () => {
    const result = buildSidebar(
      registry,
      new Set([MODULE_CODES.salesCore]),
      new Set([PERMISSIONS.sales.orderRead]),
    );

    expect(result.map((item) => item.id)).toEqual(['sales']);
  });

  it('does not mutate the registry', () => {
    const snapshot = registry.map((item) => ({
      id: item.id,
      childIds: item.children?.map((child) => child.id),
    }));

    buildSidebar(registry, [MODULE_CODES.platformCore], [], []);

    expect(
      registry.map((item) => ({
        id: item.id,
        childIds: item.children?.map((child) => child.id),
      })),
    ).toEqual(snapshot);
  });

  it('filters the real navigation registry using a session payload', () => {
    const visible = buildSidebar(
      NAVIGATION_REGISTRY,
      [MODULE_CODES.platformCore, MODULE_CODES.salesCore, MODULE_CODES.invCore],
      [PERMISSIONS.sales.orderRead],
      [SYSTEM_ROLES.salesRepresentative],
    );

    expect(visible.map((item) => item.id)).toEqual(['dashboard', 'sales']);
    expect(visible.some((item) => item.id === 'finance')).toBe(false);
    expect(visible.some((item) => item.id === 'admin')).toBe(false);
  });

  it('exposes every configured module route when all modules and roles are granted', () => {
    const allModules = Object.values(MODULE_CODES);
    const allPermissions = Object.values(PERMISSIONS).flatMap((group) => Object.values(group));

    const visible = buildSidebar(NAVIGATION_REGISTRY, allModules, allPermissions, [
      SYSTEM_ROLES.tenantAdmin,
    ]);

    const routeCount = visible.reduce(
      (count, item) => count + (item.href ? 1 : 0) + (item.children?.length ?? 0),
      0,
    );

    // 10 module landings + 7 module sub-routes + 2 admin routes.
    expect(routeCount).toBe(19);
  });

  it('wires every module sub-route into the registry with its module and permission gate', () => {
    const allModules = Object.values(MODULE_CODES);
    const allPermissions = Object.values(PERMISSIONS).flatMap((group) => Object.values(group));

    const visible = buildSidebar(NAVIGATION_REGISTRY, allModules, allPermissions, [
      SYSTEM_ROLES.tenantAdmin,
    ]);

    const childIds = (id: string) =>
      visible.find((item) => item.id === id)?.children?.map((child) => child.id);

    expect(childIds('procurement')).toEqual([
      'procurement-suppliers',
      'procurement-purchase-orders',
    ]);
    expect(childIds('manufacturing')).toEqual(['manufacturing-work-orders', 'manufacturing-bom']);
    expect(childIds('hris')).toEqual(['hris-attendance']);
    expect(childIds('projects')).toEqual(['projects-tasks']);
    expect(childIds('assets')).toEqual(['assets-register']);

    const attendance = visible
      .find((item) => item.id === 'hris')
      ?.children?.find((child) => child.id === 'hris-attendance');

    expect(attendance?.moduleCode).toBe(MODULE_CODES.hrisAttendance);
    expect(attendance?.permission).toBe(PERMISSIONS.hris.attendanceRead);
  });

  it('hides sub-routes when their own license module is inactive', () => {
    const visible = buildSidebar(
      NAVIGATION_REGISTRY,
      [MODULE_CODES.platformCore, MODULE_CODES.hrisCore],
      [PERMISSIONS.hris.attendanceRead],
      [SYSTEM_ROLES.tenantAdmin],
    );

    const hris = visible.find((item) => item.id === 'hris');

    expect(hris?.href).toBe('/hris');
    expect(hris?.children).toBeUndefined();
  });
});

describe('isNavItemActive', () => {
  it('matches the exact route and nested routes', () => {
    const sales = registry.find((item) => item.id === 'sales');

    expect(sales).toBeDefined();
    expect(isNavItemActive(sales as NavItem, '/sales')).toBe(true);
    expect(isNavItemActive(sales as NavItem, '/sales/orders/42')).toBe(true);
    expect(isNavItemActive(sales as NavItem, '/sales-archive')).toBe(false);
  });

  it('activates a parent when any child route is active', () => {
    const admin = registry.find((item) => item.id === 'admin');

    expect(admin).toBeDefined();
    expect(isNavItemActive(admin as NavItem, '/admin/security')).toBe(true);
    expect(isNavItemActive(admin as NavItem, '/dashboard')).toBe(false);
  });
});
