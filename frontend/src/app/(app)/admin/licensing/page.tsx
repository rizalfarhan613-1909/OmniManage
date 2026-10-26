import type { Metadata } from 'next';

import { LicensingView } from '@/features/admin/components/LicensingView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.admin.licensing') };

export default function LicensingPage() {
  return <LicensingView />;
}
