import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { ModuleOverview } from '@/components/shared/ModuleOverview';
import { MODULE_CODES } from '@/config/module-codes';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.manufacturing') };

export default function ManufacturingPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.mfgCore}>
      <ModuleOverview
        titleKey="nav.manufacturing"
        descriptionKey="page.manufacturing.description"
      />
    </ModuleGuard>
  );
}
