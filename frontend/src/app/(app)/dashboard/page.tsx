import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { DashboardView } from '@/features/dashboard/components/DashboardView';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'nav.dashboard') };
}

export default function DashboardPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.platformCore}>
      <DashboardView />
    </ModuleGuard>
  );
}
