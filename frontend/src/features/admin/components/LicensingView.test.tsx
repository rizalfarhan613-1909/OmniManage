import { fireEvent, render, screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import type { TenantSummary, ToggleTenantModuleInput } from '@/features/admin/api/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { LicensingView } from './LicensingView';

const api = vi.hoisted(() => ({
  useTenants: vi.fn(),
  useToggleTenantModule: vi.fn(),
}));

vi.mock('@/features/admin/api/use-tenants', () => ({
  useTenants: api.useTenants,
}));

vi.mock('@/features/admin/api/use-toggle-tenant-module', () => ({
  useToggleTenantModule: api.useToggleTenantModule,
}));

const ACME: TenantSummary = {
  id: 'tenant-1',
  code: 'ACME',
  name: 'Acme Trading',
  status: 'Active',
  planCode: 'Business',
  activeUserCount: 12,
  activeUserQuota: 25,
  licensedModules: ['FIN_GL', 'SALES_CORE'],
  createdAt: '2026-01-04T00:00:00Z',
};

const GLOBEX: TenantSummary = {
  id: 'tenant-2',
  code: 'GLBX',
  name: 'Globex Manufacturing',
  status: 'Suspended',
  planCode: 'Enterprise',
  activeUserCount: 40,
  activeUserQuota: 50,
  licensedModules: [],
  createdAt: '2026-02-10T00:00:00Z',
};

function renderView(tenants: TenantSummary[] = [ACME, GLOBEX]) {
  api.useTenants.mockReturnValue(queryResult({ data: pageOf(tenants) }));
  return render(<LicensingView />);
}

beforeEach(() => {
  api.useToggleTenantModule.mockReturnValue(mutationResult({ mutate: vi.fn() }));
});

describe('LicensingView', () => {
  it('renders the tenant directory with tier and active user quota', () => {
    renderView();

    expect(screen.getByText('Acme Trading')).toBeInTheDocument();
    expect(screen.getByText('ACME')).toBeInTheDocument();
    expect(screen.getByText('Business')).toBeInTheDocument();
    expect(screen.getByText('12 of 25')).toBeInTheDocument();
    expect(screen.getByText('Globex Manufacturing')).toBeInTheDocument();
    expect(screen.getAllByText('Active').length).toBeGreaterThan(0);
  });

  it('turns an inactive module ON through the switch and announces success', () => {
    const mutate = vi.fn(
      (
        _variables: ToggleTenantModuleInput,
        options?: { onSuccess?: () => void; onSettled?: () => void },
      ) => {
        options?.onSuccess?.();
        options?.onSettled?.();
      },
    );
    api.useToggleTenantModule.mockReturnValue(mutationResult({ mutate }));

    renderView();

    const receivableSwitch = screen.getByRole('switch', {
      name: 'Toggle Accounts Receivable for Acme Trading',
    });
    expect(receivableSwitch).toHaveAttribute('aria-checked', 'false');

    fireEvent.click(receivableSwitch);

    expect(mutate).toHaveBeenCalledWith(
      {
        tenantId: 'tenant-1',
        moduleCode: 'FIN_AR',
        isActive: true,
        reason: 'Enabled through the Superadmin licensing console.',
      },
      expect.objectContaining({
        onSuccess: expect.any(Function),
        onSettled: expect.any(Function),
      }),
    );
    expect(
      screen.getByText('Accounts Receivable is now enabled for Acme Trading.'),
    ).toBeInTheDocument();
  });

  it('requires a reason before turning an active module OFF', () => {
    const mutate = vi.fn(
      (
        _variables: ToggleTenantModuleInput,
        options?: { onSuccess?: () => void; onSettled?: () => void },
      ) => {
        options?.onSuccess?.();
        options?.onSettled?.();
      },
    );
    api.useToggleTenantModule.mockReturnValue(mutationResult({ mutate }));

    renderView();

    fireEvent.click(
      screen.getByRole('switch', {
        name: 'Toggle General Ledger for Acme Trading',
      }),
    );

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', { name: 'Disable General Ledger?' }),
    ).toBeInTheDocument();

    const confirmButton = within(dialog).getByRole('button', { name: 'Disable module' });
    expect(confirmButton).toBeDisabled();
    expect(mutate).not.toHaveBeenCalled();

    fireEvent.change(within(dialog).getByLabelText('Reason for change'), {
      target: { value: 'Budget review for next quarter' },
    });
    fireEvent.click(confirmButton);

    expect(mutate).toHaveBeenCalledWith(
      {
        tenantId: 'tenant-1',
        moduleCode: 'FIN_GL',
        isActive: false,
        reason: 'Budget review for next quarter',
      },
      expect.objectContaining({ onSuccess: expect.any(Function) }),
    );
    expect(
      screen.getByText('General Ledger is now disabled for Acme Trading.'),
    ).toBeInTheDocument();
  });

  it('blocks enabling a dependent module while its hard dependency is OFF', () => {
    renderView([
      {
        ...ACME,
        licensedModules: ['SALES_CORE'],
      },
    ]);

    const receivableSwitch = screen.getByRole('switch', {
      name: 'Toggle Accounts Receivable for Acme Trading',
    });

    expect(receivableSwitch).toBeDisabled();
    expect(receivableSwitch).toHaveAccessibleDescription('Requires General Ledger');
    expect(screen.getAllByText('Requires General Ledger')).toHaveLength(2);
  });

  it('renders the loading skeleton for the directory', () => {
    api.useTenants.mockReturnValue(queryResult({ isLoading: true }));

    render(<LicensingView />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });

  it('renders the empty state when no tenants exist', () => {
    renderView([]);

    expect(screen.getByText('No tenants yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh directory' })).toBeInTheDocument();
  });

  it('renders the error state with a copyable traceId', () => {
    api.useTenants.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'Tenant directory unavailable.',
          status: 500,
          traceId: 'trace-licensing-500',
        }),
      }),
    );

    render(<LicensingView />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-licensing-500')).toBeInTheDocument();
  });
});
