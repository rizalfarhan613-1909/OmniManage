import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { InventoryView } from '@/features/inventory/components/InventoryView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.inventory') };

export default function InventoryPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.invCore} permission={PERMISSIONS.inventory.stockRead}>
      <InventoryView />
    </ModuleGuard>
  );
}
