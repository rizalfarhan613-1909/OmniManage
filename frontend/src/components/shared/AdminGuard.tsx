import { redirect } from 'next/navigation';
import type { ReactNode } from 'react';

import { ForbiddenState } from '@/components/shared/ForbiddenState';
import { ModuleNotLicensedState } from '@/components/shared/ModuleNotLicensedState';
import { MODULE_CODES } from '@/config/module-codes';
import { getSession } from '@/lib/auth/session';
import { hasAdminConsoleAccess, isSystemSuperadmin } from '@/lib/auth/roles';

type Props = {
  children: ReactNode;
};

/**
 * Route-group guard for the administrative consoles.
 *
 * The role check runs BEFORE the license check so a non-admin is always shown
 * `ForbiddenState` (DESIGN.md §7) instead of a module-license message. The
 * platform realm is not tenant-licensed, so Superadmins bypass the tenant
 * `PLATFORM_CORE` check.
 */
export async function AdminGuard({ children }: Props) {
  const session = await getSession();

  if (!session) {
    redirect('/login');
  }

  if (!hasAdminConsoleAccess(session.user.roles)) {
    return <ForbiddenState />;
  }

  if (
    !isSystemSuperadmin(session.user.roles) &&
    !session.activeModules.includes(MODULE_CODES.platformCore)
  ) {
    return <ModuleNotLicensedState moduleCode={MODULE_CODES.platformCore} />;
  }

  return <>{children}</>;
}
