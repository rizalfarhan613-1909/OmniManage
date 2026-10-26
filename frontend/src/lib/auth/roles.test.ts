import { describe, expect, it } from 'vitest';

import { NAVIGATION_REGISTRY } from '@/config/navigation.registry';
import { MODULE_CODES } from '@/config/module-codes';
import { SYSTEM_ROLES } from '@/config/roles';
import { hasAdminConsoleAccess, isSystemSuperadmin } from '@/lib/auth/roles';
import { buildSidebar } from '@/lib/navigation/buildSidebar';

describe('admin console role gate', () => {
  it('admits tenant administrators', () => {
    expect(hasAdminConsoleAccess([SYSTEM_ROLES.tenantAdmin])).toBe(true);
  });

  it('admits system superadmins', () => {
    expect(hasAdminConsoleAccess([SYSTEM_ROLES.systemSuperadmin])).toBe(true);
  });

  it('rejects every non-administrative role', () => {
    expect(
      hasAdminConsoleAccess([
        SYSTEM_ROLES.financeManager,
        SYSTEM_ROLES.hrManager,
        SYSTEM_ROLES.salesRepresentative,
        SYSTEM_ROLES.warehouseOperator,
        SYSTEM_ROLES.productionManager,
        SYSTEM_ROLES.generalStaff,
      ]),
    ).toBe(false);
    expect(hasAdminConsoleAccess([])).toBe(false);
  });

  it('identifies the platform superadmin realm', () => {
    expect(isSystemSuperadmin([SYSTEM_ROLES.systemSuperadmin])).toBe(true);
    expect(isSystemSuperadmin([SYSTEM_ROLES.tenantAdmin])).toBe(false);
  });
});

describe('admin navigation gate', () => {
  it('hides the admin console from non-admin roles', () => {
    const visible = buildSidebar(
      NAVIGATION_REGISTRY,
      [MODULE_CODES.platformCore],
      [],
      [SYSTEM_ROLES.financeManager],
    );

    expect(visible.some((item) => item.id === 'admin')).toBe(false);
  });

  it('shows both admin routes to a system superadmin', () => {
    const visible = buildSidebar(
      NAVIGATION_REGISTRY,
      [MODULE_CODES.platformCore],
      [],
      [SYSTEM_ROLES.systemSuperadmin],
    );

    const admin = visible.find((item) => item.id === 'admin');

    expect(admin).toBeDefined();
    expect(admin?.children?.map((item) => item.id)).toEqual(['admin-licensing', 'admin-security']);
  });
});
