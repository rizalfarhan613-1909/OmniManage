import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { PurchaseOrdersView } from '@/features/procurement/components/PurchaseOrdersView';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'procurement.orders.title') };
}

/** Mirrors `[RequireModule(ModuleCodes.ProcCore)]` + PO read on PurchaseOrdersController. */
export default function PurchaseOrdersPage() {
  return (
    <ModuleGuard
      moduleCode={MODULE_CODES.procCore}
      permission={PERMISSIONS.procurement.purchaseOrderRead}
    >
      <PurchaseOrdersView />
    </ModuleGuard>
  );
}
