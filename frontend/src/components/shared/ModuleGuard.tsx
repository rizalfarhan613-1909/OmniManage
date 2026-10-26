import { redirect } from 'next/navigation';
import type { ReactNode } from 'react';

import { ForbiddenState } from '@/components/shared/ForbiddenState';
import { ModuleNotLicensedState } from '@/components/shared/ModuleNotLicensedState';
import type { ModuleCode } from '@/config/module-codes';
import type { PermissionCode } from '@/config/permissions';
import type { SystemRole } from '@/config/roles';
import { getSession } from '@/lib/auth/session';

type Props = {
  moduleCode: ModuleCode;
  permission?: PermissionCode;
  roles?: readonly SystemRole[];
  children: ReactNode;
};

/**
 * Route-group guard required by Rules.md §10.2. A missing license renders
 * `ModuleNotLicensedState` (not a 404); a missing permission renders
 * `ForbiddenState`. The backend repeats both checks on every request.
 */
export async function ModuleGuard({ moduleCode, permission, roles, children }: Props) {
  const session = await getSession();

  if (!session) {
    redirect('/login');
  }

  if (!session.activeModules.includes(moduleCode)) {
    return <ModuleNotLicensedState moduleCode={moduleCode} />;
  }

  if (permission && !session.permissions.includes(permission)) {
    return <ForbiddenState />;
  }

  if (roles && roles.length > 0 && !roles.some((role) => session.user.roles.includes(role))) {
    return <ForbiddenState />;
  }

  return <>{children}</>;
}
