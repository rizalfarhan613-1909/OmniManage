import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { WorkOrdersView } from '@/features/manufacturing/components/WorkOrdersView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('manufacturing.workOrders.title') };

/** Mirrors `[RequireModule(ModuleCodes.MfgCore)]` + work-order read on WorkOrdersController. */
export default function WorkOrdersPage() {
  return (
    <ModuleGuard
      moduleCode={MODULE_CODES.mfgCore}
      permission={PERMISSIONS.manufacturing.workOrderRead}
    >
      <WorkOrdersView />
    </ModuleGuard>
  );
}
