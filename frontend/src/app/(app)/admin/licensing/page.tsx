import type { Metadata } from 'next';

import { LicensingView } from '@/features/admin/components/LicensingView';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'nav.admin.licensing') };
}

export default function LicensingPage() {
  return <LicensingView />;
}
