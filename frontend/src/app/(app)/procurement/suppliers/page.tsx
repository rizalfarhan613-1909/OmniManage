import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { SuppliersView } from '@/features/procurement/components/SuppliersView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('procurement.suppliers.title') };

/** Mirrors `[RequireModule(ModuleCodes.ProcCore)]` + supplier read on SuppliersController. */
export default function SuppliersPage() {
  return (
    <ModuleGuard
      moduleCode={MODULE_CODES.procCore}
      permission={PERMISSIONS.procurement.supplierRead}
    >
      <SuppliersView />
    </ModuleGuard>
  );
}
