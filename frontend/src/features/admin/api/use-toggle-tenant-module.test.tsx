import { renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { createFetchMock, errorResponse, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { adminKeys } from './keys';
import type { TenantModuleLicense } from './types';
import { useToggleTenantModule } from './use-toggle-tenant-module';

const sessionRefresh = vi.hoisted(() => vi.fn<() => Promise<void>>());

vi.mock('@/components/providers/SessionProvider', () => ({
  useSessionRefresh: () => sessionRefresh,
}));

const fetchMock = createFetchMock();

const LICENSE: TenantModuleLicense = {
  tenantId: 'tenant-1',
  moduleCode: 'HRIS_PAYROLL',
  isActive: true,
  activatedAt: '2026-10-22T09:00:00Z',
  expiresAt: null,
  changedBy: 'superadmin-1',
  changeReason: 'Roll out payroll for Q4',
  version: 2,
};

beforeEach(() => {
  vi.stubGlobal('fetch', fetchMock);
  sessionRefresh.mockResolvedValue(undefined);
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('useToggleTenantModule', () => {
  it('patches the license, invalidates tenants and revalidates the session', async () => {
    fetchMock.mockResolvedValue(successResponse(LICENSE));
    const queryClient = createTestQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useToggleTenantModule(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await result.current.mutateAsync({
      tenantId: 'tenant-1',
      moduleCode: 'HRIS_PAYROLL',
      isActive: true,
      reason: 'Roll out payroll for Q4',
    });

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/platform/tenants/tenant-1/modules/HRIS_PAYROLL',
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify({
          isActive: true,
          reason: 'Roll out payroll for Q4',
        }),
      }),
    );
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: adminKeys.tenants(),
    });
    expect(sessionRefresh).toHaveBeenCalledTimes(1);
  });

  it('does not refresh the session when the backend rejects the change', async () => {
    fetchMock.mockResolvedValue(
      errorResponse(
        'PLATFORM.MODULE_DEPENDENCY_UNMET',
        'FIN_GL must be active before FIN_AR.',
        409,
        'trace-license-409',
      ),
    );
    const queryClient = createTestQueryClient();

    const { result } = renderHook(() => useToggleTenantModule(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await expect(
      result.current.mutateAsync({
        tenantId: 'tenant-1',
        moduleCode: 'FIN_AR',
        isActive: true,
        reason: 'Enable receivables',
      }),
    ).rejects.toMatchObject({
      code: 'PLATFORM.MODULE_DEPENDENCY_UNMET',
      status: 409,
      traceId: 'trace-license-409',
    });
    expect(sessionRefresh).not.toHaveBeenCalled();
  });
});
