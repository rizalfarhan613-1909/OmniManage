import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { ProjectStatus, ProjectTaskStatus } from '../types';

export interface TaskQueryParams extends ListQueryParams {
  projectId?: string;
  assigneeEmployeeId?: string;
  status?: ProjectTaskStatus;
}

export interface WorkLogQueryParams extends ListQueryParams {
  taskId?: string;
  employeeId?: string;
  fromDate?: string;
  toDate?: string;
}

export interface ProjectQueryParams extends ListQueryParams {
  customerId?: string;
  status?: ProjectStatus;
}

export const projectsKeys = {
  all: [QUERY_KEY_ROOTS.projects] as const,
  tasks: () => [...projectsKeys.all, 'tasks'] as const,
  taskList: (params: TaskQueryParams) => [...projectsKeys.tasks(), 'list', params] as const,
  task: (id: string) => [...projectsKeys.tasks(), 'detail', id] as const,
  workLogs: (taskId?: string) => [...projectsKeys.all, 'work-logs', taskId ?? 'all'] as const,
  workLogList: (params: WorkLogQueryParams) =>
    [...projectsKeys.workLogs(params.taskId), 'list', params] as const,
  projects: () => [...projectsKeys.all, 'projects'] as const,
  projectList: (params: ProjectQueryParams) =>
    [...projectsKeys.projects(), 'list', params] as const,
  project: (id: string) => [...projectsKeys.projects(), 'detail', id] as const,
};
