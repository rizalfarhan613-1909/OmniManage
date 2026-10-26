import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { ModuleOverview } from '@/components/shared/ModuleOverview';
import { MODULE_CODES } from '@/config/module-codes';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.hris') };

export default function HrisPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.hrisCore}>
      <ModuleOverview titleKey="nav.hris" descriptionKey="page.hris.description" />
    </ModuleGuard>
  );
}
