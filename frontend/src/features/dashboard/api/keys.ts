import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

export const dashboardKeys = {
  all: [QUERY_KEY_ROOTS.dashboard] as const,
  summary: (scope: readonly string[]) => [...dashboardKeys.all, 'summary', scope] as const,
};
