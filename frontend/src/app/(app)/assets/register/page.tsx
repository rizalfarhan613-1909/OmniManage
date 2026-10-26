import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { t } from '@/lib/i18n';

import { AssetRegisterView } from '@/features/assets/components/AssetRegisterView';

export const metadata: Metadata = { title: t('assets.register.title') };

export default function AssetRegisterPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.assetCore} permission={PERMISSIONS.assets.assetRead}>
      <AssetRegisterView />
    </ModuleGuard>
  );
}
