import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { CreateTaskRequest } from '../types';
import { projectsKeys } from './keys';

/**
 * `POST /api/v1/projects/tasks`.
 *
 * Returns the new task identifier. Task lists and project workload views are
 * refreshed because they derive their progress from the task aggregate.
 */
export function useCreateTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateTaskRequest) =>
      apiClient.post<string>('/api/v1/projects/tasks', request).then((response) => response.data),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: projectsKeys.tasks() }),
        queryClient.invalidateQueries({ queryKey: projectsKeys.projects() }),
        queryClient.invalidateQueries({ queryKey: [QUERY_KEY_ROOTS.dashboard] }),
      ]),
  });
}
