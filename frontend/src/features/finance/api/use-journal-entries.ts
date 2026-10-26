import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import { financeKeys, type JournalEntryQueryParams } from './keys';
import type { JournalEntry } from './types';

export function useJournalEntries(
  params: JournalEntryQueryParams = {},
  options: QueryHookOptions = {},
) {
  return useQuery({
    queryKey: financeKeys.journalList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<JournalEntry>>(`/api/v1/finance/journal-entries${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
