import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { RecordWorkLogRequest } from '../types';
import { projectsKeys } from './keys';

export interface RecordWorkLogInput {
  taskId: string;
  request: RecordWorkLogRequest;
}

/**
 * `POST /api/v1/projects/tasks/{taskId}/work-logs`.
 *
 * The backend commits the work log, task progress and (when completing the
 * task) the outbox event together, so every derived task/project view is
 * invalidated after success.
 */
export function useRecordWorkLog() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ taskId, request }: RecordWorkLogInput) =>
      apiClient
        .post<string>(`/api/v1/projects/tasks/${taskId}/work-logs`, request)
        .then((response) => response.data),
    onSuccess: (_workLogId, { taskId }) =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: projectsKeys.tasks() }),
        queryClient.invalidateQueries({ queryKey: projectsKeys.task(taskId) }),
        queryClient.invalidateQueries({ queryKey: projectsKeys.workLogs(taskId) }),
        queryClient.invalidateQueries({ queryKey: projectsKeys.projects() }),
        queryClient.invalidateQueries({ queryKey: [QUERY_KEY_ROOTS.dashboard] }),
      ]),
  });
}
