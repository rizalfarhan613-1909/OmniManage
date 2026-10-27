import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

import { TasksView } from '@/features/projects/components/TasksView';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'projects.tasks.title') };
}

export default function TasksPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.projectCore} permission={PERMISSIONS.projects.taskRead}>
      <TasksView />
    </ModuleGuard>
  );
}
