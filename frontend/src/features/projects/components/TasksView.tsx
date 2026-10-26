'use client';

import { Plus, ArrowUpDown } from 'lucide-react';
import { useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { PERMISSIONS } from '@/config/permissions';
import { useGetProjectTasks } from '@/features/projects/api/useGetProjectTasks';
import { useCreateTask } from '@/features/projects/api/useCreateTask';
import { useRecordWorkLog } from '@/features/projects/api/useRecordWorkLog';
import type {
  TaskDto,
  ProjectTaskStatus,
  CreateTaskRequest,
  RecordWorkLogRequest,
} from '@/features/projects/types';
import { toSortParam, type SortState } from '@/lib/api/pagination';
import { t, type TranslationKey } from '@/lib/i18n';
import { formatQuantity } from '@/lib/utils/format';

import { CreateTaskDialog } from './CreateTaskDialog';
import { WorkLogDialog } from './WorkLogDialog';

const PAGE_SIZE = 10;

const STATUS_TONE: Record<ProjectTaskStatus, StatusTone> = {
  Todo: 'neutral',
  InProgress: 'info',
  Blocked: 'warning',
  Done: 'success',
};

const STATUS_LABEL = {
  Todo: 'projects.tasks.status.todo',
  InProgress: 'projects.tasks.status.inProgress',
  Blocked: 'projects.tasks.status.blocked',
  Done: 'projects.tasks.status.done',
} as const;

export function TasksView() {
  const session = useSession();
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'createdAt', direction: 'desc' });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isWorkLogOpen, setIsWorkLogOpen] = useState<{ taskId: string; taskTitle: string } | null>(
    null,
  );
  const [lastCreated, setLastCreated] = useState<TranslationKey | null>(null);
  const [lastWorkLog, setLastWorkLog] = useState<TranslationKey | null>(null);

  const canCreateTask = session.permissions.includes(PERMISSIONS.projects.taskCreate);
  const canLogWork = session.permissions.includes(PERMISSIONS.projects.workLogCreate);

  const tasksQuery = useGetProjectTasks({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });

  const createTaskMutation = useCreateTask();
  const workLogMutation = useRecordWorkLog();

  const handleOpenCreate = () => {
    createTaskMutation.reset();
    setIsCreateOpen(true);
  };

  const handleCloseCreate = () => {
    if (!createTaskMutation.isPending) {
      setIsCreateOpen(false);
    }
  };

  const handleCreateTask = (request: CreateTaskRequest) => {
    createTaskMutation.mutate(request, {
      onSuccess: () => {
        setLastCreated('projects.tasks.create.success');
        setIsCreateOpen(false);
      },
    });
  };

  const handleOpenWorkLog = (taskId: string, taskTitle: string) => {
    setIsWorkLogOpen({ taskId, taskTitle });
  };

  const handleCloseWorkLog = () => {
    setIsWorkLogOpen(null);
  };

  const handleWorkLog = (request: RecordWorkLogRequest) => {
    workLogMutation.mutate(
      { taskId: isWorkLogOpen?.taskId ?? '', request },
      {
        onSuccess: () => {
          setLastWorkLog('projects.tasks.workLog.success');
          handleCloseWorkLog();
        },
      },
    );
  };

  const columns: DataTableColumn<TaskDto>[] = [
    {
      id: 'title',
      header: t('projects.tasks.task'),
      sortable: true,
      cell: (task) => <span className="font-medium text-charcoal">{task.title}</span>,
    },
    {
      id: 'projectId',
      header: t('projects.tasks.project'),
      sortable: true,
      cell: (task) => (
        <span className="font-mono text-xs text-charcoal-light">{task.projectId}</span>
      ),
    },
    {
      id: 'assigneeEmployeeId',
      header: t('projects.tasks.assignee'),
      sortable: true,
      cell: (task) => (
        <span className="font-mono text-xs text-charcoal-light">
          {task.assigneeEmployeeId ?? '—'}
        </span>
      ),
    },
    {
      id: 'status',
      header: t('projects.tasks.status'),
      sortable: true,
      cell: (task) => (
        <StatusBadge
          label={t(STATUS_LABEL[task.status])}
          tone={STATUS_TONE[task.status] ?? 'neutral'}
        />
      ),
    },
    {
      id: 'estimatedHours',
      header: t('projects.tasks.estimatedHours'),
      sortable: true,
      align: 'right',
      className: 'tabular-nums',
      cell: (task) => formatQuantity(task.estimatedHours),
    },
    {
      id: 'completedHours',
      header: t('projects.tasks.completedHours'),
      sortable: true,
      align: 'right',
      className: 'tabular-nums',
      cell: (task) => formatQuantity(task.completedHours),
    },
    {
      id: 'actions',
      header: t('projects.tasks.actions'),
      align: 'right',
      cell: (task) =>
        canLogWork ? (
          <Button
            variant="ghost"
            size="sm"
            aria-label={t('projects.tasks.actions.logWork')}
            onClick={() => handleOpenWorkLog(task.id, task.title)}
          >
            <ArrowUpDown className="h-3 w-3" aria-hidden="true" />
          </Button>
        ) : (
          <span aria-hidden="true" className="text-charcoal-light">
            &mdash;
          </span>
        ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('projects.tasks.title')}
        description={t('projects.tasks.description')}
        actions={
          canCreateTask ? (
            <Button onClick={handleOpenCreate}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              {t('projects.tasks.create.submit')}
            </Button>
          ) : null
        }
      />

      {lastCreated ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <Plus className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t(lastCreated)}</p>
        </div>
      ) : null}

      {lastWorkLog ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <ArrowUpDown className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t(lastWorkLog)}</p>
        </div>
      ) : null}

      <QuerySection
        query={tasksQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('projects.tasks.empty.title')}
        emptyDescription={t('projects.tasks.empty.body')}
        emptyActionLabel={t('projects.tasks.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(task) => task.id}
            caption={t('projects.tasks.tableCaption')}
            sort={sort}
            onSortChange={(next) => {
              setSort(next);
              setPage(1);
            }}
            pagination={{
              page: data.page,
              pageSize: data.pageSize,
              totalItems: data.totalItems,
              totalPages: data.totalPages,
            }}
            onPageChange={setPage}
          />
        )}
      </QuerySection>

      <CreateTaskDialog
        isOpen={isCreateOpen}
        isPending={createTaskMutation.isPending}
        error={createTaskMutation.error}
        onConfirm={handleCreateTask}
        onCancel={handleCloseCreate}
      />

      {isWorkLogOpen && (
        <WorkLogDialog
          isOpen
          taskId={isWorkLogOpen.taskId}
          taskTitle={isWorkLogOpen.taskTitle}
          isPending={workLogMutation.isPending}
          error={workLogMutation.error}
          onConfirm={handleWorkLog}
          onCancel={handleCloseWorkLog}
        />
      )}
    </div>
  );
}
