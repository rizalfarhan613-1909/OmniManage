import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { procurementKeys, type PurchaseOrderQueryParams } from './keys';
import type { PurchaseOrder } from './types';

/**
 * `GET /api/v1/procurement/purchase-orders`.
 *
 * Supports server-side paging, sorting and the documented status/supplier
 * filters. The response already carries the supplier name snapshot, so the
 * table does not need a second lookup query.
 */
export function useGetPurchaseOrders(
  params: PurchaseOrderQueryParams = {},
  options: QueryHookOptions = {},
) {
  return useQuery({
    queryKey: procurementKeys.purchaseOrderList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<PurchaseOrder>>(
          `/api/v1/procurement/purchase-orders${toQueryString(params)}`,
          { signal },
        )
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
