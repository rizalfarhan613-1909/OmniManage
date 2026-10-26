import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { SalesView } from '@/features/sales/components/SalesView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.sales') };

export default function SalesPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.salesCore} permission={PERMISSIONS.sales.orderRead}>
      <SalesView />
    </ModuleGuard>
  );
}
