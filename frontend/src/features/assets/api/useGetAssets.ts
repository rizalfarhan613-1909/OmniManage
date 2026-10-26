import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import type { AssetDto } from '../types';
import { assetsKeys, type AssetQueryParams } from './keys';

/** `GET /api/v1/assets` — paginated asset register. */
export function useGetAssets(params: AssetQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: assetsKeys.assetList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<AssetDto>>(`/api/v1/assets${toQueryString(params)}`, { signal })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
