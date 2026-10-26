import type { ReactNode } from 'react';

import { AdminGuard } from '@/components/shared/AdminGuard';

type Props = {
  children: ReactNode;
};

/** Restricts every `/admin/**` route to TENANT_ADMIN / SYSTEM_SUPERADMIN. */
export default function AdminLayout({ children }: Props) {
  return <AdminGuard>{children}</AdminGuard>;
}
