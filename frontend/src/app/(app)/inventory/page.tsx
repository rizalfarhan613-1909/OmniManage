import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { InventoryView } from '@/features/inventory/components/InventoryView';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'nav.inventory') };
}

export default function InventoryPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.invCore} permission={PERMISSIONS.inventory.stockRead}>
      <InventoryView />
    </ModuleGuard>
  );
}
