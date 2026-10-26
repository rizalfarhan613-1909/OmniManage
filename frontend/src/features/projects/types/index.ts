/**
 * Wire types for the Projects module (Phase 6).
 *
 * Mirrors the presentation contracts and domain aggregates on the backend.
 * Following Rules.md §7, decimal values travel as fixed-scale strings while
 * dates use ISO-8601 (`DateOnly` values are `yyyy-MM-dd`).
 */

/** Mirrors `ProjectStatus`. */
export type ProjectStatus = 'Planned' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled';

/** Mirrors `ProjectTaskStatus`. */
export type ProjectTaskStatus = 'Todo' | 'InProgress' | 'Blocked' | 'Done';

/** `POST /api/v1/projects/tasks` request body. */
export interface CreateTaskRequest {
  projectId: string;
  title: string;
  assigneeEmployeeId?: string | null;
  estimatedHours: number;
}

/** `POST /api/v1/projects/tasks/{taskId}/work-logs` request body. */
export interface RecordWorkLogRequest {
  employeeId: string;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  loggedDate: string;
  hoursSpent: number;
  description: string;
  completeTask: boolean;
}

/** Mirrors `ProjectTask`. */
export interface TaskDto {
  id: string;
  projectId: string;
  title: string;
  assigneeEmployeeId: string | null;
  status: ProjectTaskStatus;
  estimatedHours: string;
  completedHours: string;
}

/** Mirrors `WorkLog`. */
export interface WorkLogDto {
  id: string;
  projectTaskId: string;
  employeeId: string;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  loggedDate: string;
  hoursSpent: string;
  description: string;
}

/** Mirrors `Project`. */
export interface ProjectDto {
  id: string;
  code: string;
  name: string;
  description: string | null;
  customerId: string;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  startDate: string;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  endDate: string | null;
  status: ProjectStatus;
}
