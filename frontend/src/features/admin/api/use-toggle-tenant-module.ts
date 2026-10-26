import { useMutation, useQueryClient } from '@tanstack/react-query';

import { useSessionRefresh } from '@/components/providers/SessionProvider';
import { apiClient } from '@/lib/api/client';

import { adminKeys } from './keys';
import type { TenantModuleLicense, ToggleTenantModuleInput } from './types';

/**
 * `PATCH /api/v1/platform/tenants/{id}/modules/{code}` (UC-02).
 *
 * Licensing changes are never optimistic (DESIGN.md §3.4): the switch waits
 * for server confirmation. On success the tenant directory is invalidated and
 * `/api/auth/session` is revalidated so the Dynamic Sidebar updates immediately.
 */
export function useToggleTenantModule() {
  const queryClient = useQueryClient();
  const refreshSession = useSessionRefresh();

  return useMutation({
    mutationFn: ({ tenantId, moduleCode, isActive, reason }: ToggleTenantModuleInput) =>
      apiClient
        .patch<TenantModuleLicense>(`/api/v1/platform/tenants/${tenantId}/modules/${moduleCode}`, {
          isActive,
          reason,
        })
        .then((response) => response.data),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: adminKeys.tenants() }),
        refreshSession(),
      ]);
    },
  });
}
