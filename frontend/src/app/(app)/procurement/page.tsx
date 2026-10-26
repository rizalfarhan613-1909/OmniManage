import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { ModuleOverview } from '@/components/shared/ModuleOverview';
import { MODULE_CODES } from '@/config/module-codes';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.procurement') };

export default function ProcurementPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.procCore}>
      <ModuleOverview titleKey="nav.procurement" descriptionKey="page.procurement.description" />
    </ModuleGuard>
  );
}
