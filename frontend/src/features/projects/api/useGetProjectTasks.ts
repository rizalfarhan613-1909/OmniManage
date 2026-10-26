import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import type { TaskDto } from '../types';
import { projectsKeys, type TaskQueryParams } from './keys';

/** `GET /api/v1/projects/tasks` — paginated task list, filterable by project. */
export function useGetProjectTasks(params: TaskQueryParams = {}, options: QueryHookOptions = {}) {
  return useQuery({
    queryKey: projectsKeys.taskList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<TaskDto>>(`/api/v1/projects/tasks${toQueryString(params)}`, { signal })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
