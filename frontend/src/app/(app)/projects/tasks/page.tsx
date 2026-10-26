import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { t } from '@/lib/i18n';

import { TasksView } from '@/features/projects/components/TasksView';

export const metadata: Metadata = { title: t('projects.tasks.title') };

export default function TasksPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.projectCore} permission={PERMISSIONS.projects.taskRead}>
      <TasksView />
    </ModuleGuard>
  );
}
