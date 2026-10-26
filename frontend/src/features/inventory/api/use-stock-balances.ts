import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { inventoryKeys, type StockBalanceQueryParams } from './keys';
import type { StockBalance } from './types';

export function useStockBalances(
  params: StockBalanceQueryParams = {},
  options: QueryHookOptions = {},
) {
  return useQuery({
    queryKey: inventoryKeys.stockBalances(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<StockBalance>>(
          `/api/v1/inventory/stock/balances${toQueryString(params)}`,
          { signal },
        )
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
