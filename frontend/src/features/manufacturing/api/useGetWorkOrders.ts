import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { manufacturingKeys, type WorkOrderQueryParams } from './keys';
import type { WorkOrder } from './types';

/**
 * `GET /api/v1/manufacturing/work-orders`.
 *
 * Supports server-side paging, sorting plus the documented status/product/BOM
 * filters. The response already carries the BOM code snapshot, so the table
 * does not need a second lookup query.
 */
export function useGetWorkOrders(
  params: WorkOrderQueryParams = {},
  options: QueryHookOptions = {},
) {
  return useQuery({
    queryKey: manufacturingKeys.workOrderList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<WorkOrder>>(`/api/v1/manufacturing/work-orders${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
