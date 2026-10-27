import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { ModuleOverview } from '@/components/shared/ModuleOverview';
import { MODULE_CODES } from '@/config/module-codes';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'nav.assets') };
}

/** Mirrors `[RequireModule(ModuleCodes.AssetCore)]` on the Assets controllers. */
export default function AssetsPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.assetCore}>
      <ModuleOverview titleKey="nav.assets" descriptionKey="page.assets.description" />
    </ModuleGuard>
  );
}
