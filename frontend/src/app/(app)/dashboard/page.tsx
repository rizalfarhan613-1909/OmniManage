import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { DashboardView } from '@/features/dashboard/components/DashboardView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.dashboard') };

export default function DashboardPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.platformCore}>
      <DashboardView />
    </ModuleGuard>
  );
}
