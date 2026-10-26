import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { salesKeys, type SalesOrderQueryParams } from './keys';
import type { SalesOrder } from './types';

export function useSalesOrders(params: SalesOrderQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: salesKeys.orderList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<SalesOrder>>(`/api/v1/sales/orders${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
