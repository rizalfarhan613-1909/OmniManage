import { redirect } from 'next/navigation';
import type { ReactNode } from 'react';

import { QueryProvider } from '@/components/providers/QueryProvider';
import { SessionProvider } from '@/components/providers/SessionProvider';
import { AppShell } from '@/components/shared/AppShell';
import { getSession } from '@/lib/auth/session';

type Props = {
  children: ReactNode;
};

export default async function AppLayout({ children }: Props) {
  const session = await getSession();

  if (!session) {
    redirect('/login');
  }

  return (
    <QueryProvider>
      <SessionProvider initialSession={session}>
        <AppShell>{children}</AppShell>
      </SessionProvider>
    </QueryProvider>
  );
}
