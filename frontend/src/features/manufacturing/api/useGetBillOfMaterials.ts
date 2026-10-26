import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { manufacturingKeys, type BillOfMaterialQueryParams } from './keys';
import type { BillOfMaterial } from './types';

/**
 * `GET /api/v1/manufacturing/bom`.
 *
 * Returns the versioned product structures with their component snapshots.
 * Active-only filtering is used by the work-order dialog so a draft BOM cannot
 * be selected for production.
 */
export function useGetBillOfMaterials(
  params: BillOfMaterialQueryParams = {},
  options: QueryHookOptions = {},
) {
  return useQuery({
    queryKey: manufacturingKeys.billOfMaterialList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<BillOfMaterial>>(`/api/v1/manufacturing/bom${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
