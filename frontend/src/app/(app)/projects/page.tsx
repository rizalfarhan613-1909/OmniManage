import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { ModuleOverview } from '@/components/shared/ModuleOverview';
import { MODULE_CODES } from '@/config/module-codes';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.projects') };

/** Mirrors `[RequireModule(ModuleCodes.ProjectCore)]` on the Projects controllers. */
export default function ProjectsPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.projectCore}>
      <ModuleOverview titleKey="nav.projects" descriptionKey="page.projects.description" />
    </ModuleGuard>
  );
}
