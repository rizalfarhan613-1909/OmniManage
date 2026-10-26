import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { ModuleOverview } from '@/components/shared/ModuleOverview';
import { MODULE_CODES } from '@/config/module-codes';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.assets') };

/** Mirrors `[RequireModule(ModuleCodes.AssetCore)]` on the Assets controllers. */
export default function AssetsPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.assetCore}>
      <ModuleOverview titleKey="nav.assets" descriptionKey="page.assets.description" />
    </ModuleGuard>
  );
}
