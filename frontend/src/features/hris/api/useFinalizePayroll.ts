import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { PayrollFinalizeRequest } from '../types';
import { hrisKeys } from './keys';

/**
 * `POST /api/v1/hris/payroll/runs/{id}/finalize`.
 *
 * Finalization is an irreversible financial transition, so the cache is
 * invalidated after the server confirms rather than updated optimistically.
 * The resulting `PayrollFinalized` event also posts a GL journal, hence the
 * finance and dashboard invalidations.
 */
export function useFinalizePayroll() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ payrollRunId }: PayrollFinalizeRequest) =>
      apiClient
        .post<null>(`/api/v1/hris/payroll/runs/${payrollRunId}/finalize`)
        .then(() => undefined),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: hrisKeys.payrollRuns() }),
        queryClient.invalidateQueries({ queryKey: [QUERY_KEY_ROOTS.finance] }),
        queryClient.invalidateQueries({ queryKey: [QUERY_KEY_ROOTS.dashboard] }),
      ]),
  });
}
