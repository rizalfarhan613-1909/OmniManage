import type { Metadata } from 'next';

import { AuditSecurityView } from '@/features/admin/components/AuditSecurityView';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return { title: translate(locale, 'nav.admin.security') };
}

export default function SecurityPage() {
  return <AuditSecurityView />;
}
