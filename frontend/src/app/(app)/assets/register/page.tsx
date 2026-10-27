import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

import { AssetRegisterView } from '@/features/assets/components/AssetRegisterView';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'assets.register.title') };
}

export default function AssetRegisterPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.assetCore} permission={PERMISSIONS.assets.assetRead}>
      <AssetRegisterView />
    </ModuleGuard>
  );
}
