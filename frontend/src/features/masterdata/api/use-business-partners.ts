import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { ListQueryParams, PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { masterDataKeys } from './keys';
import type { BusinessPartner } from './types';

export function useBusinessPartners(params: ListQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: masterDataKeys.businessPartnerList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<BusinessPartner>>(
          `/api/v1/masterdata/business-partners${toQueryString(params)}`,
          { signal },
        )
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
