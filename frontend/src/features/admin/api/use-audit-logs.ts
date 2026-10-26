import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { adminKeys, type AuditLogQueryParams } from './keys';
import type { AuditLogEntry } from './types';

/** `GET /api/v1/platform/audit-logs` — immutable audit trail (Security.md §6). */
export function useAuditLogs(params: AuditLogQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: adminKeys.auditLogList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<AuditLogEntry>>(`/api/v1/platform/audit-logs${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
