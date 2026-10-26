import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { BillOfMaterialsView } from '@/features/manufacturing/components/BillOfMaterialsView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('manufacturing.bom.title') };

/** Mirrors `[RequireModule(ModuleCodes.MfgCore)]` + BOM read on BillOfMaterialsController. */
export default function BillOfMaterialsPage() {
  return (
    <ModuleGuard
      moduleCode={MODULE_CODES.mfgCore}
      permission={PERMISSIONS.manufacturing.billOfMaterialRead}
    >
      <BillOfMaterialsView />
    </ModuleGuard>
  );
}
