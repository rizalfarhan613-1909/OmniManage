import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { financeKeys, type ArInvoiceQueryParams } from './keys';
import type { ArInvoice } from './types';

export function useArInvoices(params: ArInvoiceQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: financeKeys.invoiceList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<ArInvoice>>(`/api/v1/finance/invoices${toQueryString(params)}`, { signal })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
