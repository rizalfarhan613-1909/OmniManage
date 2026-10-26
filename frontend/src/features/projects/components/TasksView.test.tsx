import { fireEvent, render, screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { RecordWorkLogInput } from '@/features/projects/api/useRecordWorkLog';
import type { CreateTaskRequest, TaskDto } from '@/features/projects/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { createSession, pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { TasksView } from './TasksView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useGetProjectTasks: vi.fn(),
  useCreateTask: vi.fn(),
  useRecordWorkLog: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/projects/api/useGetProjectTasks', () => ({
  useGetProjectTasks: api.useGetProjectTasks,
}));

vi.mock('@/features/projects/api/useCreateTask', () => ({
  useCreateTask: api.useCreateTask,
}));

vi.mock('@/features/projects/api/useRecordWorkLog', () => ({
  useRecordWorkLog: api.useRecordWorkLog,
}));

const TODO: TaskDto = {
  id: 'task-1',
  projectId: 'proj-1',
  title: 'Draft API contract',
  assigneeEmployeeId: null,
  status: 'Todo',
  estimatedHours: '8.00',
  completedHours: '0.00',
};

const IN_PROGRESS: TaskDto = {
  id: 'task-2',
  projectId: 'proj-2',
  title: 'Wire authentication',
  assigneeEmployeeId: 'emp-1',
  status: 'InProgress',
  estimatedHours: '12.50',
  completedHours: '4.25',
};

/** Row-scoped access keeps row actions unambiguous once a dialog is open. */
function rowFor(taskTitle: string): HTMLElement {
  const row = screen.getByText(taskTitle).closest('tr');

  if (!row) {
    throw new Error(`No table row was rendered for the task "${taskTitle}"`);
  }

  return row;
}

beforeEach(() => {
  api.useSession.mockReturnValue(
    createSession({
      activeModules: [MODULE_CODES.projectCore],
      permissions: [
        PERMISSIONS.projects.taskRead,
        PERMISSIONS.projects.taskCreate,
        PERMISSIONS.projects.workLogCreate,
      ],
    }),
  );
  api.useGetProjectTasks.mockReturnValue(queryResult({ data: pageOf([TODO, IN_PROGRESS]) }));
  api.useCreateTask.mockReturnValue(mutationResult());
  api.useRecordWorkLog.mockReturnValue(mutationResult());
});

describe('TasksView', () => {
  it('renders tasks with localized statuses and quantity-formatted hour columns', () => {
    render(<TasksView />);

    expect(screen.getByRole('table', { name: 'Project tasks' })).toBeInTheDocument();
    expect(screen.getByText('Draft API contract')).toBeInTheDocument();
    expect(screen.getByText('Wire authentication')).toBeInTheDocument();
    expect(screen.getByText('proj-1')).toBeInTheDocument();
    expect(screen.getByText('emp-1')).toBeInTheDocument();
    expect(screen.getByText('To do')).toBeInTheDocument();
    expect(screen.getByText('In progress')).toBeInTheDocument();
    expect(screen.getByText('8')).toBeInTheDocument();
    expect(screen.getByText('12.5')).toBeInTheDocument();
    expect(screen.getByText('4.25')).toBeInTheDocument();
  });

  it('offers the work-log action on every row with the work-log permission', () => {
    render(<TasksView />);

    expect(screen.getAllByRole('button', { name: 'Record work log' })).toHaveLength(2);
  });

  it('hides the work-log and create actions without their permissions', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.projectCore],
        permissions: [PERMISSIONS.projects.taskRead],
      }),
    );

    render(<TasksView />);

    expect(screen.queryByRole('button', { name: 'Record work log' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Create task' })).not.toBeInTheDocument();
    expect(within(rowFor('Draft API contract')).getAllByText('—')).toHaveLength(2);
  });

  it('records a work log through the dialog and announces success', () => {
    const mutate = vi.fn(
      (input: RecordWorkLogInput, options?: { onSuccess?: (workLogId: string) => void }) => {
        expect(input.taskId).toBe('task-1');
        expect(input.request).toEqual({
          employeeId: 'emp-9',
          loggedDate: expect.any(String),
          hoursSpent: 4,
          description: '',
          completeTask: false,
        });
        options?.onSuccess?.('wl-1');
      },
    );
    api.useRecordWorkLog.mockReturnValue(mutationResult({ mutate }));

    render(<TasksView />);

    fireEvent.click(
      within(rowFor('Draft API contract')).getByRole('button', { name: 'Record work log' }),
    );

    const dialog = screen.getByRole('dialog');

    fireEvent.change(within(dialog).getByLabelText('Employee'), { target: { value: 'emp-9' } });
    fireEvent.change(within(dialog).getByLabelText('Hours spent'), { target: { value: '4' } });
    fireEvent.click(within(dialog).getByRole('button', { name: 'Save work log' }));

    expect(mutate).toHaveBeenCalledTimes(1);
    expect(screen.getByText('Work log recorded.')).toBeInTheDocument();
  });

  it('creates a task through the dialog with the parsed estimate', () => {
    const mutate = vi.fn(
      (request: CreateTaskRequest, options?: { onSuccess?: (taskId: string) => void }) => {
        expect(request).toEqual({
          projectId: 'proj-9',
          title: 'New task',
          assigneeEmployeeId: null,
          estimatedHours: 6,
        });
        options?.onSuccess?.('task-9');
      },
    );
    api.useCreateTask.mockReturnValue(mutationResult({ mutate }));

    render(<TasksView />);

    fireEvent.click(screen.getByRole('button', { name: 'Create task' }));

    const dialog = screen.getByRole('dialog');

    fireEvent.change(within(dialog).getByLabelText('Project'), { target: { value: 'proj-9' } });
    fireEvent.change(within(dialog).getByLabelText('Title'), { target: { value: 'New task' } });
    fireEvent.change(within(dialog).getByLabelText('Estimated hours'), {
      target: { value: '6' },
    });
    fireEvent.click(within(dialog).getByRole('button', { name: 'Create task' }));

    expect(mutate).toHaveBeenCalledTimes(1);
    expect(screen.getByText('Task created successfully.')).toBeInTheDocument();
  });

  it('renders the loading skeleton while the task list loads', () => {
    api.useGetProjectTasks.mockReturnValue(queryResult({ isLoading: true }));

    render(<TasksView />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });

  it('renders the empty state with a refresh CTA', () => {
    api.useGetProjectTasks.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<TasksView />);

    expect(screen.getByText('No tasks yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh tasks' })).toBeInTheDocument();
  });

  it('renders the error state with the traceId', () => {
    api.useGetProjectTasks.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'The task list could not be loaded.',
          status: 500,
          traceId: 'trace-projects-500',
        }),
      }),
    );

    render(<TasksView />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-projects-500')).toBeInTheDocument();
  });

  it('renders forbidden when the API rejects the task read permission', () => {
    api.useGetProjectTasks.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.forbidden,
          message: 'Forbidden.',
          status: 403,
          traceId: 'trace-projects-403',
        }),
      }),
    );

    render(<TasksView />);

    expect(screen.getByText("You don't have access")).toBeInTheDocument();
  });
});
