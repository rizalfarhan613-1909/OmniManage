import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { FinanceView } from '@/features/finance/components/FinanceView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.finance') };

/**
 * The route requires the GL license. A/R has its own license + permission and
 * renders its own not-licensed/forbidden state inside the view, so users with
 * A/R access are not blocked by a missing GL permission.
 */
export default function FinancePage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.finGl}>
      <FinanceView />
    </ModuleGuard>
  );
}
