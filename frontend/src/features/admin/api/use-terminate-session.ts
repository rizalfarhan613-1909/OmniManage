import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';

import { adminKeys } from './keys';
import type { TerminateSessionResult } from './types';

/**
 * `DELETE /api/v1/platform/sessions/{id}`.
 *
 * The backend revokes the whole refresh-token family for the session; the list
 * is refetched only after the server confirms.
 */
export function useTerminateSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (sessionId: string) =>
      apiClient
        .delete<TerminateSessionResult>(`/api/v1/platform/sessions/${sessionId}`)
        .then((response) => response.data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: adminKeys.sessions() }),
  });
}
