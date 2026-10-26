import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { ListQueryParams, PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { masterDataKeys } from './keys';
import type { Item } from './types';

export function useItems(params: ListQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: masterDataKeys.itemList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<Item>>(`/api/v1/masterdata/items${toQueryString(params)}`, { signal })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
