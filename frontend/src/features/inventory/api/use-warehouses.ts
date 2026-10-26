import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { ListQueryParams, PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { inventoryKeys } from './keys';
import type { Warehouse } from './types';

export interface WarehouseQueryParams extends ListQueryParams {
  isActive?: boolean;
}

export function useWarehouses(params: WarehouseQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: inventoryKeys.warehouseList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<Warehouse>>(`/api/v1/inventory/warehouses${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
