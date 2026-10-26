import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { SYSTEM_ROLES } from '@/config/roles';
import { t } from '@/lib/i18n';
import type { Session } from '@/lib/auth/types';

import { AdminGuard } from './AdminGuard';

const auth = vi.hoisted(() => ({
  getSession: vi.fn(),
  redirect: vi.fn(),
}));

vi.mock('@/lib/auth/session', () => ({
  getSession: auth.getSession,
}));

vi.mock('next/navigation', () => ({
  redirect: auth.redirect,
}));

function session(roles: string[], activeModules: string[] = []): Session {
  return {
    user: { id: 'user-1', email: 'admin@acme.test', roles },
    tenant: { id: 'tenant-1', code: 'ACME', name: 'Acme Trading' },
    activeModules,
    permissions: [],
  };
}

async function renderGuard() {
  render(await AdminGuard({ children: <p>Admin console</p> }));
}

describe('AdminGuard', () => {
  beforeEach(() => {
    auth.getSession.mockReset();
    auth.redirect.mockReset();
    auth.redirect.mockImplementation(() => {
      throw new Error('NEXT_REDIRECT');
    });
  });

  it('redirects unauthenticated visitors to the login page', async () => {
    auth.getSession.mockResolvedValue(null);

    await expect(AdminGuard({ children: null })).rejects.toThrow('NEXT_REDIRECT');
    expect(auth.redirect).toHaveBeenCalledWith('/login');
  });

  it('shows ForbiddenState to a non-admin role', async () => {
    auth.getSession.mockResolvedValue(session([SYSTEM_ROLES.financeManager]));

    await renderGuard();

    expect(screen.getByText(t('states.forbidden.title'))).toBeInTheDocument();
    expect(screen.queryByText('Admin console')).not.toBeInTheDocument();
  });

  it('shows the not-licensed state to a tenant admin without PLATFORM_CORE', async () => {
    auth.getSession.mockResolvedValue(session([SYSTEM_ROLES.tenantAdmin]));

    await renderGuard();

    expect(screen.getByText(t('states.moduleNotLicensed.title'))).toBeInTheDocument();
    expect(screen.getByText(MODULE_CODES.platformCore, { exact: false })).toBeInTheDocument();
  });

  it('admits a tenant admin when PLATFORM_CORE is active', async () => {
    auth.getSession.mockResolvedValue(
      session([SYSTEM_ROLES.tenantAdmin], [MODULE_CODES.platformCore]),
    );

    await renderGuard();

    expect(screen.getByText('Admin console')).toBeInTheDocument();
  });

  it('lets a system superadmin bypass the tenant license check', async () => {
    auth.getSession.mockResolvedValue(session([SYSTEM_ROLES.systemSuperadmin]));

    await renderGuard();

    expect(screen.getByText('Admin console')).toBeInTheDocument();
  });
});
