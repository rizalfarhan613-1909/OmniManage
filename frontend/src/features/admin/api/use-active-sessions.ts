import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { ListQueryParams, PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { adminKeys } from './keys';
import type { ActiveUserSession } from './types';

/** `GET /api/v1/platform/sessions` — live sessions for the security console. */
export function useActiveSessions(params: ListQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: adminKeys.sessionList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<ActiveUserSession>>(`/api/v1/platform/sessions${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
