import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { SalesView } from '@/features/sales/components/SalesView';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'nav.sales') };
}

export default function SalesPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.salesCore} permission={PERMISSIONS.sales.orderRead}>
      <SalesView />
    </ModuleGuard>
  );
}
