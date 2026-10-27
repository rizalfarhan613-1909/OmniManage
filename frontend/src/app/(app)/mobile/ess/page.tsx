import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { ModuleOverview } from '@/components/shared/ModuleOverview';
import { MODULE_CODES } from '@/config/module-codes';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'nav.ess') };
}

export default function EssPage() {
  return (
    <ModuleGuard moduleCode={MODULE_CODES.hrisEss}>
      <ModuleOverview titleKey="nav.ess" descriptionKey="page.ess.description" />
    </ModuleGuard>
  );
}
