import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { ClockInRequest } from '../types';
import { hrisKeys } from './keys';

/**
 * `POST /api/v1/hris/attendance/clock-in`.
 *
 * The backend appends the attendance row and the outbox event in one
 * transaction and returns the new attendance log identifier.
 */
export function useClockIn() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ClockInRequest) =>
      apiClient
        .post<string>('/api/v1/hris/attendance/clock-in', request)
        .then((response) => response.data),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: hrisKeys.attendanceLogs() }),
        queryClient.invalidateQueries({ queryKey: [QUERY_KEY_ROOTS.dashboard] }),
      ]),
  });
}
