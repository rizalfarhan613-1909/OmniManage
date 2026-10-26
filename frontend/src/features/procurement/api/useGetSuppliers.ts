import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { procurementKeys, type SupplierQueryParams } from './keys';
import type { Supplier } from './types';

/**
 * `GET /api/v1/procurement/suppliers`.
 *
 * Supplier master is owned by the Procurement module. The list is server-paged
 * and sorted so the table never loads the whole vendor directory.
 */
export function useGetSuppliers(params: SupplierQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: procurementKeys.supplierList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<Supplier>>(`/api/v1/procurement/suppliers${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
