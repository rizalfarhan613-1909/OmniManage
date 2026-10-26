import type { Metadata } from 'next';

import { AuditSecurityView } from '@/features/admin/components/AuditSecurityView';
import { t } from '@/lib/i18n';

export const metadata: Metadata = { title: t('nav.admin.security') };

export default function SecurityPage() {
  return <AuditSecurityView />;
}
