import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { ListQueryParams, PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { adminKeys } from './keys';
import type { TenantSummary } from './types';

/** `GET /api/v1/platform/tenants` — Superadmin tenant master directory. */
export function useTenants(params: ListQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: adminKeys.tenantList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<TenantSummary>>(`/api/v1/platform/tenants${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
